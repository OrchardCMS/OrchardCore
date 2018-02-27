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
            var mailMessage = new MailMessage
            {
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };

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
                return SmtpResult.Failed(S["An error occured while sending an email: '{0}'", e.Message]);
            }
        }

        private SmtpClient GetClient()
        {
            if (String.IsNullOrWhiteSpace(_options.Host))
            {
                return new SmtpClient();
            }

            var smtp = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

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

            return smtp;
        }
    }
}
