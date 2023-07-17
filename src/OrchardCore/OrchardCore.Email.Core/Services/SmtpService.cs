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
    public class SmtpService : ISmtpService
    {
        private const string EmailExtension = ".eml";

        private static readonly char[] _emailsSeparator = new char[] { ',', ';' };

        private readonly SmtpSettings _options;
        private readonly ILogger _logger;
        protected readonly IStringLocalizer S;

        /// <summary>
        /// Initializes a new instance of a <see cref="SmtpService"/>.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{SmtpSettings}"/>.</param>
        /// <param name="logger">The <see cref="ILogger{SmtpService}"/>.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer{SmtpService}"/>.</param>
        public SmtpService(
            IOptions<SmtpSettings> options,
            ILogger<SmtpService> logger,
            IStringLocalizer<SmtpService> stringLocalizer)
        {
            _options = options.Value;
            _logger = logger;
            S = stringLocalizer;
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A <see cref="SmtpResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
        /// <remarks>This method allows to send an email without setting <see cref="MailMessage.To"/> if <see cref="MailMessage.Cc"/> or <see cref="MailMessage.Bcc"/> is provided.</remarks>
        public async Task<SmtpResult> SendAsync(MailMessage message)
        {
            if (_options == null)
            {
                return SmtpResult.Failed(S["SMTP settings must be configured before an email can be sent."]);
            }

            SmtpResult result;
            var response = default(string);
            try
            {
                // Set the MailMessage.From, to avoid the confusion between _options.DefaultSender (Author) and submitter (Sender)
                var senderAddress = String.IsNullOrWhiteSpace(message.From)
                    ? _options.DefaultSender
                    : message.From;

                if (!String.IsNullOrWhiteSpace(senderAddress))
                {
                    message.From = senderAddress;
                }

                var errors = new List<LocalizedString>();

                var mimeMessage = FromMailMessage(message, errors);

                if (errors.Count > 0)
                {
                    return SmtpResult.Failed(errors.ToArray());
                }

                if (mimeMessage.To.Count == 0 && mimeMessage.Cc.Count == 0 && mimeMessage.Bcc.Count == 0)
                {
                    return SmtpResult.Failed(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
                }

                switch (_options.DeliveryMethod)
                {
                    case SmtpDeliveryMethod.Network:
                        response = await SendOnlineMessageAsync(mimeMessage);
                        break;
                    case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                        await SendOfflineMessageAsync(mimeMessage, _options.PickupDirectoryLocation);
                        break;
                    default:
                        throw new NotSupportedException($"The '{_options.DeliveryMethod}' delivery method is not supported.");
                }

                result = SmtpResult.Success;
            }
            catch (Exception ex)
            {
                result = SmtpResult.Failed(S["An error occurred while sending an email: '{0}'", ex.Message]);
            }

            result.Response = response;

            return result;
        }

        private MimeMessage FromMailMessage(MailMessage message, IList<LocalizedString> errors)
        {
            var submitterAddress = String.IsNullOrWhiteSpace(message.Sender)
                ? _options.DefaultSender
                : message.Sender;

            var mimeMessage = new MimeMessage();

            if (!String.IsNullOrEmpty(submitterAddress))
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

            if (!String.IsNullOrWhiteSpace(message.From))
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

            if (!String.IsNullOrWhiteSpace(message.To))
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

            if (!String.IsNullOrWhiteSpace(message.Cc))
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

            if (!String.IsNullOrWhiteSpace(message.Bcc))
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

            if (String.IsNullOrWhiteSpace(message.ReplyTo))
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

        private bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            const string LogErrorMessage = "SMTP Server's certificate {CertificateSubject} issued by {CertificateIssuer} " +
                "with thumbprint {CertificateThumbprint} and expiration date {CertificateExpirationDate} " +
                "is considered invalid with {SslPolicyErrors} policy errors";

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            _logger.LogError(LogErrorMessage,
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

            return false;
        }

        protected virtual Task OnMessageSendingAsync(SmtpClient client, MimeMessage message) => Task.CompletedTask;

        private async Task<string> SendOnlineMessageAsync(MimeMessage message)
        {
            var secureSocketOptions = SecureSocketOptions.Auto;

            if (!_options.AutoSelectEncryption)
            {
                secureSocketOptions = _options.EncryptionMethod switch
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

            await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions);

            if (_options.RequireCredentials)
            {
                if (_options.UseDefaultCredentials)
                {
                    // There's no notion of 'UseDefaultCredentials' in MailKit, so empty credentials is passed in
                    await client.AuthenticateAsync(String.Empty, String.Empty);
                }
                else if (!String.IsNullOrWhiteSpace(_options.UserName))
                {
                    await client.AuthenticateAsync(_options.UserName, _options.Password);
                }
            }

            if (!String.IsNullOrEmpty(_options.ProxyHost))
            {
                client.ProxyClient = new Socks5Client(_options.ProxyHost, _options.ProxyPort);
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
    }
}
