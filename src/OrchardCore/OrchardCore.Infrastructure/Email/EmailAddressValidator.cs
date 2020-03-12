using MimeKit;
using OrchardCore.Email;

namespace OrchardCore.Infrastructure.Email
{
    public class EmailAddressValidator : IEmailAddressValidator
    {
        public bool Validate(string emailAddress)
            => emailAddress.IndexOf('@') > -1 && MailboxAddress.TryParse(emailAddress, out _);
    }
}
