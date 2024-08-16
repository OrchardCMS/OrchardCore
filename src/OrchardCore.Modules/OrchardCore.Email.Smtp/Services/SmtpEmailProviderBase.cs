using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using MailKit.Net.Proxy;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace OrchardCore.Email.Smtp.Services;

public abstract class SmtpEmailProviderBase : IEmailProvider
{
    private const string EmailExtension = ".eml";

    private readonly SmtpOptions _providerOptions;
    private readonly IEmailAddressValidator _emailAddressValidator;
    private readonly ILogger _logger;

    protected readonly IStringLocalizer S;

    public SmtpEmailProviderBase(
        SmtpOptions options,
        IEmailAddressValidator emailAddressValidator,
        ILogger logger,
        IStringLocalizer stringLocalizer)
    {
        _providerOptions = options;
        _emailAddressValidator = emailAddressValidator;
        _logger = logger;
        S = stringLocalizer;
    }

    public abstract LocalizedString DisplayName { get; }

    public virtual async Task<EmailResult> SendAsync(MailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_providerOptions.IsEnabled)
        {
            return EmailResult.FailedResult(S["The SMTP Email Provider is disabled."]);
        }

        var senderAddress = string.IsNullOrWhiteSpace(message.From)
            ? _providerOptions.DefaultSender
            : message.From;

        _logger.LogDebug("Attempting to send email to {Email}.", message.To);

        // Set the MailMessage.From, to avoid the confusion between DefaultSender (Author) and submitter (Sender).
        if (!string.IsNullOrWhiteSpace(senderAddress))
        {
            if (!_emailAddressValidator.Validate(senderAddress))
            {
                return EmailResult.FailedResult(nameof(message.From), S["Invalid email address for the sender: '{0}'.", senderAddress]);
            }

            message.From = senderAddress;
        }

        var mimeMessage = GetMimeMessage(message);

        try
        {
            if (_providerOptions.DeliveryMethod == SmtpDeliveryMethod.Network)
            {
                var response = await SendOnlineMessageAsync(mimeMessage);

                return EmailResult.GetSuccessResult(response);
            }

            if (_providerOptions.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory)
            {
                await SendOfflineMessageAsync(mimeMessage, _providerOptions.PickupDirectoryLocation);

                return EmailResult.SuccessResult;
            }

            throw new NotSupportedException($"The '{_providerOptions.DeliveryMethod}' delivery method is not supported.");
        }
        catch (Exception ex)
        {
            return EmailResult.FailedResult([S["An error occurred while sending an email: '{0}'", ex.Message]]);
        }
    }

    private MimeMessage GetMimeMessage(MailMessage message)
    {
        var mimeMessage = new MimeMessage();
        var submitterAddress = string.IsNullOrWhiteSpace(message.Sender)
            ? _providerOptions.DefaultSender
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

        await client.ConnectAsync(_providerOptions.Host, _providerOptions.Port, secureSocketOptions);

        if (_providerOptions.RequireCredentials)
        {
            if (_providerOptions.UseDefaultCredentials)
            {
                // There's no notion of 'UseDefaultCredentials' in MailKit, so empty credentials is passed in.
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
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            return true;
        }

        const string logErrorMessage = "SMTP Server's certificate {CertificateSubject} issued by {CertificateIssuer} " +
                               "with thumbprint {CertificateThumbprint} and expiration date {CertificateExpirationDate} " +
                               "is considered invalid with {SslPolicyErrors} policy errors";

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
