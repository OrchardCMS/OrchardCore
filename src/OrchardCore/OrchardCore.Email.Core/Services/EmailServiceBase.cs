using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Email.Services
{
    /// <summary>
    /// Represents a contract that allows to send emails.
    /// </summary>
    public abstract class EmailServiceBase<TEmailSettings> : IEmailService
        where TEmailSettings : EmailSettings
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="EmailServiceBase{TEmailSettings}"/>.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{EmailSettings}"/>.</param>
        /// <param name="logger">The <see cref="ILogger{EmailServiceBase}"/>.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer{EmailServiceBase}"/>.</param>
        protected EmailServiceBase(
            IOptions<TEmailSettings> options,
            ILogger<EmailServiceBase<TEmailSettings>> logger,
            IStringLocalizer<EmailServiceBase<TEmailSettings>> stringLocalizer)
        {
            Settings = options.Value;
            Logger = logger;
            S = stringLocalizer;
        }

        protected TEmailSettings Settings { get; }

        protected ILogger Logger { get; }

        protected IStringLocalizer S { get; }

        /// <summary>
        /// Sends the specified message to an email server for delivery.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>A <see cref="EmailResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
        public abstract Task<EmailResult> SendAsync(MailMessage message);
    }
}
