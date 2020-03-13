using MimeKit;

namespace OrchardCore.Email
{
    public class EmailAddressValidator : IEmailAddressValidator
    {
        public bool Validate(string emailAddress)
            => emailAddress.IndexOf('@') > -1 && MailboxAddress.TryParse(emailAddress, out _);
    }
}
