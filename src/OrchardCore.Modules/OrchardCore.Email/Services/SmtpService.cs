using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
            ILogger<SmtpService> logger
            )
        {
            _options = options.Value;
            _logger = logger;
        }

        public ISmtpService WithSettings(SmtpSettings settings)
        {
            return new SmtpService(Options.Create(settings), _logger);
        }


        public async Task<SmtpResult> SendAsync(EmailMessage emailMessage)
        {
            var result = new SmtpResult();
            if (emailMessage.Recipients.Length == 0)
            {
                result.ErrorMessage = "Email message doesn't have any recipient";
                _logger.LogError(result.ErrorMessage);
                return result;
            }

            var mailMessage = new MailMessage
            {
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = true
            };

            try
            {
                foreach (var recipient in ParseRecipients(emailMessage.Recipients))
                {
                    mailMessage.To.Add(new MailAddress(recipient));
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.Cc))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.Cc))
                    {
                        mailMessage.CC.Add(new MailAddress(recipient));
                    }
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.Bcc))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.Bcc))
                    {
                        mailMessage.Bcc.Add(new MailAddress(recipient));
                    }
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.From))
                {
                    mailMessage.From = new MailAddress(emailMessage.From);
                }
                else
                {
                    mailMessage.From = new MailAddress(_options.DefaultSender);
                }

                if (!String.IsNullOrWhiteSpace(emailMessage.ReplyTo))
                {
                    foreach (var recipient in ParseRecipients(emailMessage.ReplyTo))
                    {
                        mailMessage.ReplyToList.Add(new MailAddress(recipient));
                    }
                }

                if (emailMessage.Attachments != null && emailMessage.Attachments.Count > 0)
                {
                    foreach (var attachment in emailMessage.Attachments)
                    {
                        emailMessage.Attachments.Add(attachment);
                    }
                }

                await GetClient().SendMailAsync(mailMessage);
                result.Success = true;

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                _logger.LogError(e, "Could not send email");
            }

            return result;

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



        private IEnumerable<string> ParseRecipients(string recipients)
        {
            return recipients.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
