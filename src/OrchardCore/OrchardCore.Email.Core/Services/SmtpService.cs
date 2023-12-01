using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace OrchardCore.Email.Services
{
    /// <summary>
    /// Represents a SMTP service that allows to send emails.
    /// </summary>
    [Obsolete("This class is deprecated, please use SmtpEmailService instead.", true)]
    public class SmtpService : ISmtpService
    {
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
        }

        /// <summary>
        /// Sends the specified message to an SMTP server for delivery.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A <see cref="SmtpResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
        /// <remarks>This method allows to send an email without setting <see cref="MailMessage.To"/> if <see cref="MailMessage.Cc"/> or <see cref="MailMessage.Bcc"/> is provided.</remarks>
        public Task<SmtpResult> SendAsync(MailMessage message) => Task.FromResult(SmtpResult.Success);

        protected virtual Task OnMessageSendingAsync(SmtpClient client, MimeMessage message) => Task.CompletedTask;
    }
}
