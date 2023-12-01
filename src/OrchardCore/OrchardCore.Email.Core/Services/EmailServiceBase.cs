using System.Collections.Generic;
using System.Linq;
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
        private readonly IEmailAddressValidator _emailAddressValidator;

        /// <summary>
        /// Initializes a new instance of a <see cref="EmailServiceBase{TEmailSettings}"/>.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{EmailSettings}"/>.</param>
        /// <param name="logger">The <see cref="ILogger{EmailServiceBase}"/>.</param>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer{EmailServiceBase}"/>.</param>
        /// <param name="emailAddressValidator">The <see cref="IEmailAddressValidator"/>.</param>
        protected EmailServiceBase(
            IOptions<TEmailSettings> options,
            ILogger<EmailServiceBase<TEmailSettings>> logger,
            IStringLocalizer<EmailServiceBase<TEmailSettings>> stringLocalizer,
            IEmailAddressValidator emailAddressValidator)
        {
            Settings = options.Value;
            Logger = logger;
            S = stringLocalizer;
            _emailAddressValidator = emailAddressValidator;
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

        protected void ValidateMailMessage(MailMessage message, out List<LocalizedString> errors)
        {
            errors = [];
            var submitterAddress = string.IsNullOrWhiteSpace(message.Sender)
                ? Settings.DefaultSender
                : message.Sender;

            if (!string.IsNullOrEmpty(submitterAddress))
            {
                if (!IsValidEmail(submitterAddress))
                {
                    errors.Add(S["Invalid email address: '{0}'", submitterAddress]);
                }
            }

            errors.AddRange(message.GetSender()
                .Where(a => !IsValidEmail(a))
                .Select(a => S["Invalid email address: '{0}'", a]));

            var recipients = message.GetRecipients();

            errors.AddRange(recipients.To
                .Where(r => !IsValidEmail(r))
                .Select(r => S["Invalid email address: '{0}'", r]));

            errors.AddRange(recipients.Cc
                .Where(r => !IsValidEmail(r))
                .Select(r => S["Invalid email address: '{0}'", r]));

            errors.AddRange(recipients.Bcc
                .Where(r => !IsValidEmail(r))
                .Select(r => S["Invalid email address: '{0}'", r]));

            errors.AddRange(message.GetReplyTo()
                .Where(r => !IsValidEmail(r))
                .Select(r => S["Invalid email address: '{0}'", r]));

            if (recipients.To.Count == 0 && recipients.Cc.Count == 0 && recipients.Bcc.Count == 0)
            {
                errors.Add(S["The mail message should have at least one of these headers: To, Cc or Bcc."]);
            }
        }

        protected bool IsValidEmail(string email) => _emailAddressValidator.Validate(email);
    }
}
