using MimeKit;

namespace OrchardCore.Email
{
    /// <summary>
    /// Represents a service for e-mail address validation.
    /// </summary>
    public class EmailAddressValidator : IEmailAddressValidator
    {
        /// <inheritdoc/>
        public bool Validate(string emailAddress)
            => emailAddress?.IndexOf('@') > -1 && MailboxAddress.TryParse(emailAddress, out _);
    }
}
