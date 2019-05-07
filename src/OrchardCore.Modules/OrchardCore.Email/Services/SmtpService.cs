using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MailKit.Net.Smtp;
using MimeKit;

namespace OrchardCore.Email.Services
{
    public class SmtpService : ISmtpService
    {
        private readonly SmtpSettings _options;
        private readonly ILogger<SmtpService> _logger;
        private static readonly char[] EmailsSeparator = new char[] { ',', ';', ' ' };
        private const string EmailExtension = ".eml";

        public SmtpService(
            IOptions<SmtpSettings> options,
            ILogger<SmtpService> logger,
            IStringLocalizer<SmtpService> S
            )
        {
            _options = options.Value;
            _logger = logger;
            this.S = S;
        }

        public IStringLocalizer S { get; }

        public async Task<SmtpResult> SendAsync(MailMessage message)
        {
            if (_options?.DefaultSender == null)
            {
                return SmtpResult.Failed(S["SMTP settings must be configured before an email can be sent."]);
            }

            var mimeMessage = FromMailMessage(message);         
            try
            {
                using (var client = new SmtpClient())
                {
                    switch(_options.DeliveryMethod)
                    {
                        case SmtpDeliveryMethod.Network:
                            await SendOnlineMessage(mimeMessage, client);
                            break;
                        case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                            await SendOfflineMessage(mimeMessage, _options.PickupDirectoryLocation);
                            break;
                        default:
                            throw new NotSupportedException($"The '{_options.DeliveryMethod}' delivery method is not supported.");

                    }
                    client.Disconnect(true);

                    return SmtpResult.Success;
                }
            }
            catch (Exception ex)
            {
                return SmtpResult.Failed(S["An error occurred while sending an email: '{0}'", ex.Message]);
            }
        }

        private MimeMessage FromMailMessage(MailMessage message)
        {
            var mimeMessage = new MimeMessage
            {
                Sender = (message.From == null
                    ? new MailboxAddress(_options.DefaultSender)
                    : new MailboxAddress(message.From))
            };

            if (message.To != null)
            {
                foreach (var address in message.To.Split(EmailsSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    mimeMessage.To.Add(new MailboxAddress(address));
                }
            }

            if (message.Cc != null)
            {
                foreach (var address in message.Cc.Split(EmailsSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    mimeMessage.Cc.Add(new MailboxAddress(address));
                }
            }

            if (message.Bcc != null)
            {
                foreach (var address in message.Bcc.Split(EmailsSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    mimeMessage.Bcc.Add(new MailboxAddress(address));
                }
            }

            if (message.ReplyTo != null)
            {
                foreach (var address in message.ReplyTo.Split(EmailsSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    mimeMessage.ReplyTo.Add(new MailboxAddress(address));
                }
            }

            mimeMessage.Subject = message.Subject;
            var body = new BodyBuilder();
            if (message.IsBodyHtml)
            {
                body.HtmlBody = message.Body;
            }
            else
            {
                body.TextBody = message.Body;
            }
            mimeMessage.Body = body.ToMessageBody();

            return mimeMessage;
        }

        private async Task SendOnlineMessage(MimeMessage message, SmtpClient client)
        {
            await client.ConnectAsync(_options.Host, _options.Port, _options.EnableSsl);    
            var useDefaultCredentials = _options.RequireCredentials && _options.UseDefaultCredentials;
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
            await client.SendAsync(message);
        }

        private async Task SendOfflineMessage(MimeMessage message, string pickupDirectory)
        {
            var mailPath = Path.Combine(pickupDirectory, Guid.NewGuid().ToString() + EmailExtension);
            await message.WriteToAsync(mailPath, CancellationToken.None);
        }
    }
}
