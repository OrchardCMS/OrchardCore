using System;
using System.Collections.Generic;
using System.IO;
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

namespace OrchardCore.Email.Services;

/// <summary>
/// Represents a SMTP service that allows to send emails.
/// </summary>
public class SmtpEmailProvider : IEmailProvider
{
    public const string TechnicalName = "Smtp";

    private const string EmailExtension = ".eml";

    private static readonly char[] _emailsSeparator = [',', ';'];

    private readonly SmtpSettings _providerOptions;
    private readonly ILogger _logger;
    protected readonly IStringLocalizer S;

    /// <summary>
    /// Initializes a new instance of a <see cref="SmtpEmailProvider"/>.
    /// </summary>
    /// <param name="options">The <see cref="IOptions{SmtpSettings}"/>.</param>
    /// <param name="logger">The <see cref="ILogger{SmtpService}"/>.</param>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer{SmtpService}"/>.</param>
    public SmtpEmailProvider(
        IOptions<SmtpSettings> options,
        ILogger<SmtpEmailProvider> logger,
        IStringLocalizer<SmtpEmailProvider> stringLocalizer)
    {
        _providerOptions = options.Value;
        _logger = logger;
        S = stringLocalizer;
    }

    /// <summary>
    /// The name of the provider.
    /// </summary>
    public LocalizedString Name => S["SMTP"];

    /// <summary>
    /// Sends the specified message to an SMTP server for delivery.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <returns>A <see cref="EmailResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
    /// <remarks>This method allows to send an email without setting <see cref="MailMessage.To"/> if <see cref="MailMessage.Cc"/> or <see cref="MailMessage.Bcc"/> is provided.</remarks>
    public async Task<EmailResult> SendAsync(MailMessage message)
    {
        if (!(_providerOptions.IsEnabled ?? _providerOptions.HasValidSettings()))
        {
            return EmailResult.FailedResult(S["The SMTP Email Provider is disabled."]);
        }

        EmailResult result;
        var response = default(string);
        try
        {
            // Set the MailMessage.From, to avoid the confusion between _options.DefaultSender (Author) and submitter (Sender)
            var senderAddress = string.IsNullOrWhiteSpace(message.From)
                ? _providerOptions.DefaultSender
                : message.From;

            if (!string.IsNullOrWhiteSpace(senderAddress))
            {
                message.From = senderAddress;
            }

            var errors = new List<LocalizedString>();

            var mimeMessage = FromMailMessage(message, errors);

            if (errors.Count > 0)
            {
                return EmailResult.FailedResult(errors.ToArray());
            }

            if (mimeMessage.To.Count == 0 && mimeMessage.Cc.Count == 0 && mimeMessage.Bcc.Count == 0)
            {
                return EmailResult.FailedResult(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
            }

            switch (_providerOptions.DeliveryMethod)
            {
                case SmtpDeliveryMethod.Network:
                    response = await SendOnlineMessageAsync(mimeMessage);
                    break;
                case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                    await SendOfflineMessageAsync(mimeMessage, _providerOptions.PickupDirectoryLocation);
                    break;
                default:
                    throw new NotSupportedException($"The '{_providerOptions.DeliveryMethod}' delivery method is not supported.");
            }

            result = EmailResult.SuccessResult;
        }
        catch (Exception ex)
        {
            result = EmailResult.FailedResult(S["An error occurred while sending an email: '{0}'", ex.Message]);
        }

        result.Response = response;

        return result;
    }

    private MimeMessage FromMailMessage(MailMessage message, List<LocalizedString> errors)
    {
        var submitterAddress = string.IsNullOrWhiteSpace(message.Sender)
            ? _providerOptions.DefaultSender
            : message.Sender;

        var mimeMessage = new MimeMessage();

        if (!string.IsNullOrEmpty(submitterAddress))
        {
            if (MailboxAddress.TryParse(submitterAddress, out var mailBox))
            {
                mimeMessage.Sender = mailBox;

            }
            else
            {
                errors.Add(S["Invalid email address: '{0}'", submitterAddress]);
            }
        }

        if (!string.IsNullOrWhiteSpace(message.From))
        {
            foreach (var address in message.From.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (MailboxAddress.TryParse(address, out var mailBox))
                {
                    mimeMessage.From.Add(mailBox);
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(message.To))
        {
            foreach (var address in message.To.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (MailboxAddress.TryParse(address, out var mailBox))
                {
                    mimeMessage.To.Add(mailBox);
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(message.Cc))
        {
            foreach (var address in message.Cc.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (MailboxAddress.TryParse(address, out var mailBox))
                {
                    mimeMessage.Cc.Add(mailBox);
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(message.Bcc))
        {
            foreach (var address in message.Bcc.Split(_emailsSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (MailboxAddress.TryParse(address, out var mailBox))
                {
                    mimeMessage.Bcc.Add(mailBox);
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

        if (string.IsNullOrWhiteSpace(message.ReplyTo))
        {
            foreach (var address in mimeMessage.From)
            {
                mimeMessage.ReplyTo.Add(address);
            }
        }
        else
        {
            foreach (var address in message.ReplyTo.Split(_emailsSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (MailboxAddress.TryParse(address, out var mailBox))
                {
                    mimeMessage.ReplyTo.Add(mailBox);
                }
                else
                {
                    errors.Add(S["Invalid email address: '{0}'", address]);
                }
            }
        }

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

    protected virtual Task OnMessageSendingAsync(SmtpClient client, MimeMessage message) => Task.CompletedTask;

    private async Task<string> SendOnlineMessageAsync(MimeMessage message)
    {
        var secureSocketOptions = SecureSocketOptions.Auto;

        if (!_providerOptions.AutoSelectEncryption)
        {
            secureSocketOptions = _providerOptions.EncryptionMethod switch
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

        await client.ConnectAsync(_providerOptions.Host, _providerOptions.Port, secureSocketOptions);

        if (_providerOptions.RequireCredentials)
        {
            if (_providerOptions.UseDefaultCredentials)
            {
                // There's no notion of 'UseDefaultCredentials' in MailKit, so empty credentials is passed in
                await client.AuthenticateAsync(string.Empty, string.Empty);
            }
            else if (!string.IsNullOrWhiteSpace(_providerOptions.UserName))
            {
                await client.AuthenticateAsync(_providerOptions.UserName, _providerOptions.Password);
            }
        }

        if (!string.IsNullOrEmpty(_providerOptions.ProxyHost))
        {
            client.ProxyClient = new Socks5Client(_providerOptions.ProxyHost, _providerOptions.ProxyPort);
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

        return _providerOptions.IgnoreInvalidSslCertificate;
    }
}
