using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OrchardCore.Email.Services;

namespace OrchardCore.Email.Smtp.Services;

public class SmtpEmailDeliveryService : IEmailDeliveryService
{
    private const string EmailExtension = ".eml";

    private readonly SmtpEmailSettings _emailSettings;
    private readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public SmtpEmailDeliveryService(
        IOptions<SmtpEmailSettings> options,
        ILogger<SmtpEmailDeliveryService> logger,
        IStringLocalizer<SmtpEmailDeliveryService> stringLocalizer)
    {
        _emailSettings = options.Value;
        _logger = logger;
        S = stringLocalizer;
    }

    /// <inheritdoc/>
    /// <remarks>This method allows to send an email without setting <see cref="MailMessage.To"/> if <see cref="MailMessage.Cc"/> or <see cref="MailMessage.Bcc"/> is provided.</remarks>
    public async Task<IEmailResult> DeliverAsync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (_emailSettings == null)
        {
            return EmailResult.Failed(S["SMTP settings must be configured before an email can be sent."]);
        }

        SmtpEmailResult result;
        var response = default(string);
        try
        {
            // Set the MailMessage.From, to avoid the confusion between Options.DefaultSender (Author) and submitter (Sender).
            var senderAddress = string.IsNullOrWhiteSpace(message.From)
                ? _emailSettings.DefaultSender
                : message.From;

            if (!string.IsNullOrWhiteSpace(senderAddress))
            {
                message.From = senderAddress;
            }

            var mimeMessage = FromMailMessage(message);

            switch (_emailSettings.DeliveryMethod)
            {
                case SmtpDeliveryMethod.Network:
                    response = await SendOnlineMessageAsync(mimeMessage);
                    break;
                case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                    await SendOfflineMessageAsync(mimeMessage, _emailSettings.PickupDirectoryLocation);
                    break;
                default:
                    throw new NotSupportedException($"The '{_emailSettings.DeliveryMethod}' delivery method is not supported.");
            }

            result = new SmtpEmailResult(true);
        }
        catch (Exception ex)
        {
            result = new SmtpEmailResult(new[] { S["An error occurred while sending an email: '{0}'", ex.Message] });
        }

        result.Response = response;

        return result;
    }

    private MimeMessage FromMailMessage(MailMessage message)
    {
        var mimeMessage = new MimeMessage();
        var submitterAddress = string.IsNullOrWhiteSpace(message.Sender)
            ? _emailSettings.DefaultSender
            : message.Sender;

        if (!string.IsNullOrEmpty(submitterAddress))
        {
            mimeMessage.Sender = MailboxAddress.Parse(submitterAddress);
        }

        mimeMessage.From.AddRange(message.GetSender().Select(MailboxAddress.Parse));

        var recipients = message.GetRecipients();
        mimeMessage.To.AddRange(recipients.To.Select(MailboxAddress.Parse));
        mimeMessage.Cc.AddRange(recipients.Cc.Select(MailboxAddress.Parse));
        mimeMessage.Bcc.AddRange(recipients.Bcc.Select(MailboxAddress.Parse));

        mimeMessage.ReplyTo.AddRange(message.GetReplyTo().Select(MailboxAddress.Parse));

        mimeMessage.Subject = message.Subject;

        var body = new BodyBuilder();

        if (message.IsHtmlBody)
        {
            body.HtmlBody = message.Body;
        }
        else
        {
            body.TextBody = message.Body;
        }

        foreach (var attachment in message.Attachments)
        {
            // Stream must not be null, otherwise it would try to get the filesystem path
            if (attachment.Stream != null)
            {
                body.Attachments.Add(attachment.Filename, attachment.Stream);
            }
        }

        mimeMessage.Body = body.ToMessageBody();

        return mimeMessage;
    }

    private Task OnMessageSendingAsync(SmtpClient client, MimeMessage message) => Task.CompletedTask;

    private async Task<string> SendOnlineMessageAsync(MimeMessage message)
    {
        var secureSocketOptions = SecureSocketOptions.Auto;

        if (!_emailSettings.AutoSelectEncryption)
        {
            secureSocketOptions = _emailSettings.EncryptionMethod switch
            {
                SmtpEncryptionMethod.None => SecureSocketOptions.None,
                SmtpEncryptionMethod.SslTls => SecureSocketOptions.SslOnConnect,
                SmtpEncryptionMethod.StartTls => SecureSocketOptions.StartTls,
                _ => SecureSocketOptions.Auto,
            };
        }

        using var client = new SmtpClient();

        client.ServerCertificateValidationCallback = CertificateValidationCallback;

        await OnMessageSendingAsync(client, message);

        await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, secureSocketOptions);

        if (_emailSettings.RequireCredentials)
        {
            if (_emailSettings.UseDefaultCredentials)
            {
                // There's no notion of 'UseDefaultCredentials' in MailKit, so empty credentials is passed in.
                await client.AuthenticateAsync(string.Empty, string.Empty);
            }
            else if (!string.IsNullOrWhiteSpace(_emailSettings.UserName))
            {
                await client.AuthenticateAsync(_emailSettings.UserName, _emailSettings.Password);
            }
        }

        if (!string.IsNullOrEmpty(_emailSettings.ProxyHost))
        {
            client.ProxyClient = new Socks5Client(_emailSettings.ProxyHost, _emailSettings.ProxyPort);
        }

        var response = await client.SendAsync(message);

        await client.DisconnectAsync(true);

        return response;
    }

    private static Task SendOfflineMessageAsync(MimeMessage message, string pickupDirectory)
    {
        var mailPath = Path.Combine(pickupDirectory, Guid.NewGuid().ToString() + EmailExtension);
        return message.WriteToAsync(mailPath, CancellationToken.None);
    }

    private bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        const string logErrorMessage = "SMTP Server's certificate {CertificateSubject} issued by {CertificateIssuer} " +
                                       "with thumbprint {CertificateThumbprint} and expiration date {CertificateExpirationDate} " +
                                       "is considered invalid with {SslPolicyErrors} policy errors";

        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        _logger.LogError(logErrorMessage,
            certificate.Subject,
            certificate.Issuer,
            certificate.GetCertHashString(),
            certificate.GetExpirationDateString(),
            sslPolicyErrors);

        if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) && chain?.ChainStatus != null)
        {
            foreach (var chainStatus in chain.ChainStatus)
            {
                _logger.LogError("Status: {Status} - {StatusInformation}", chainStatus.Status, chainStatus.StatusInformation);
            }
        }

        return _emailSettings.IgnoreInvalidSslCertificate;
    }
}
