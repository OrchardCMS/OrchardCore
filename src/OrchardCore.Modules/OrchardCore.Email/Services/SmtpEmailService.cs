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

namespace OrchardCore.Email.Services
{
    /// <summary>
    /// Represents a SMTP service that allows to send emails.
    /// </summary>
    public class SmtpEmailService : EmailServiceBase<SmtpEmailSettings>
    {
        private const string EmailExtension = ".eml";

        private static readonly char[] _emailsSeparator = [',', ';'];

        /// <summary>
        /// Initializes a new instance of a <see cref="SmtpEmailService"/>.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{SmtpSettings}"/>.</param>
        /// <param name="logger">The <see cref="ILogger{SmtpService}"/>.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer{SmtpService}"/>.</param>
        /// <param name="emailAddressValidator">The <see cref="IEmailAddressValidator"/>.</param>
        public SmtpEmailService(
            IOptions<SmtpEmailSettings> options,
            ILogger<SmtpEmailService> logger,
            IStringLocalizer<SmtpEmailService> stringLocalizer,
            IEmailAddressValidator emailAddressValidator) : base(options, logger, stringLocalizer, emailAddressValidator)
        {
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A <see cref="SmtpEmailResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
        /// <remarks>This method allows to send an email without setting <see cref="MailMessage.To"/> if <see cref="MailMessage.Cc"/> or <see cref="MailMessage.Bcc"/> is provided.</remarks>
        public override async Task<EmailResult> SendAsync(MailMessage message)
        {
            if (Settings == null)
            {
                return EmailResult.Failed(S["SMTP settings must be configured before an email can be sent."]);
            }

            SmtpEmailResult result;
            var response = default(string);
            try
            {
                // Set the MailMessage.From, to avoid the confusion between Options.DefaultSender (Author) and submitter (Sender)
                var senderAddress = string.IsNullOrWhiteSpace(message.From)
                    ? Settings.DefaultSender
                    : message.From;

                if (!string.IsNullOrWhiteSpace(senderAddress))
                {
                    message.From = senderAddress;
                }

                var errors = new List<LocalizedString>();

                var mimeMessage = FromMailMessage(message, errors);

                if (errors.Count > 0)
                {
                    return EmailResult.Failed(errors.ToArray());
                }

                if (mimeMessage.To.Count == 0 && mimeMessage.Cc.Count == 0 && mimeMessage.Bcc.Count == 0)
                {
                    return EmailResult.Failed(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
                }

                switch (Settings.DeliveryMethod)
                {
                    case SmtpDeliveryMethod.Network:
                        response = await SendOnlineMessageAsync(mimeMessage);
                        break;
                    case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                        await SendOfflineMessageAsync(mimeMessage, Settings.PickupDirectoryLocation);
                        break;
                    default:
                        throw new NotSupportedException($"The '{Settings.DeliveryMethod}' delivery method is not supported.");
                }

                result = new SmtpEmailResult(true);
            }
            catch (Exception ex)
            {
                result = new SmtpEmailResult(new [] { S["An error occurred while sending an email: '{0}'", ex.Message] });
            }

            result.Response = response;

            return result;
        }

        private MimeMessage FromMailMessage(MailMessage message, IList<LocalizedString> errors)
        {
            var submitterAddress = string.IsNullOrWhiteSpace(message.Sender)
                ? Settings.DefaultSender
                : message.Sender;

            var mimeMessage = new MimeMessage();

            if (!string.IsNullOrEmpty(submitterAddress))
            {
                if (IsValidEmail(submitterAddress))
                {
                    mimeMessage.Sender = MailboxAddress.Parse(submitterAddress);

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
                    if (IsValidEmail(address))
                    {
                        mimeMessage.From.Add(MailboxAddress.Parse(address));
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
                    if (IsValidEmail(address))
                    {
                        mimeMessage.To.Add(MailboxAddress.Parse(address));
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
                    if (IsValidEmail(address))
                    {
                        mimeMessage.Cc.Add(MailboxAddress.Parse(address));
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
                    if (IsValidEmail(address))
                    {
                        mimeMessage.Bcc.Add(MailboxAddress.Parse(address));
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
                    if (IsValidEmail(address))
                    {
                        mimeMessage.ReplyTo.Add(MailboxAddress.Parse(address));
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

        private Task OnMessageSendingAsync(SmtpClient client, MimeMessage message) => Task.CompletedTask;

        private async Task<string> SendOnlineMessageAsync(MimeMessage message)
        {
            var secureSocketOptions = SecureSocketOptions.Auto;

            if (!Settings.AutoSelectEncryption)
            {
                secureSocketOptions = Settings.EncryptionMethod switch
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

            await client.ConnectAsync(Settings.Host, Settings.Port, secureSocketOptions);

            if (Settings.RequireCredentials)
            {
                if (Settings.UseDefaultCredentials)
                {
                    // There's no notion of 'UseDefaultCredentials' in MailKit, so empty credentials is passed in
                    await client.AuthenticateAsync(string.Empty, string.Empty);
                }
                else if (!string.IsNullOrWhiteSpace(Settings.UserName))
                {
                    await client.AuthenticateAsync(Settings.UserName, Settings.Password);
                }
            }

            if (!string.IsNullOrEmpty(Settings.ProxyHost))
            {
                client.ProxyClient = new Socks5Client(Settings.ProxyHost, Settings.ProxyPort);
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

            Logger.LogError(logErrorMessage,
                certificate.Subject,
                certificate.Issuer,
                certificate.GetCertHashString(),
                certificate.GetExpirationDateString(),
                sslPolicyErrors);

            if (sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors) && chain?.ChainStatus != null)
            {
                foreach (var chainStatus in chain.ChainStatus)
                {
                    Logger.LogError("Status: {Status} - {StatusInformation}", chainStatus.Status, chainStatus.StatusInformation);
                }
            }

            return Settings.IgnoreInvalidSslCertificate;
        }
    }
}
