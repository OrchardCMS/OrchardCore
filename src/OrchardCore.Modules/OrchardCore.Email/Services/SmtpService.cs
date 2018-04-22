using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Services
{
    public class SmtpService : ISmtpService
    {
        private readonly SmtpSettings _options;
        private readonly ILogger<SmtpService> _logger;

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

            if (message.From == null)
            {
                message.From = new MailAddress(_options.DefaultSender);
            }
            
            try
            {
                using (var client = GetClient())
                {
                    await client.SendMailAsync(message);
                    return SmtpResult.Success;
                }
            }
            catch (Exception e)
            {
                return SmtpResult.Failed(S["An error occurred while sending an email: '{0}'", e.Message]);
            }
        }

        private SmtpClient GetClient()
        {
            var smtp = new SmtpClient()
            {
                DeliveryMethod = _options.DeliveryMethod
            };
            
            switch (smtp.DeliveryMethod)
            {
                case SmtpDeliveryMethod.Network:
                    smtp.Host = _options.Host;
                    smtp.Port = _options.Port;
                    smtp.EnableSsl = _options.EnableSsl;

                    smtp.UseDefaultCredentials = _options.RequireCredentials && _options.UseDefaultCredentials;

                    if (_options.RequireCredentials)
                    {
                        if (_options.UseDefaultCredentials)
                        {
                            smtp.UseDefaultCredentials = true;
                        }
                        else if (!String.IsNullOrWhiteSpace(_options.UserName))
                        {
                            smtp.Credentials = new NetworkCredential(_options.UserName, _options.Password);
                        }
                    }

                    break;

                case SmtpDeliveryMethod.PickupDirectoryFromIis:
                    // Nothing to configure
                    break;

                case SmtpDeliveryMethod.SpecifiedPickupDirectory:
                    smtp.PickupDirectoryLocation = _options.PickupDirectoryLocation;
                    break;

                default:
                    throw new NotSupportedException($"The '{smtp.DeliveryMethod}' delivery method is not supported."); ;
            }

            return smtp;
        }
    }
}
