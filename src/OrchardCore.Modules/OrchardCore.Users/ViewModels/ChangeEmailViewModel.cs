using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MimeKit;

namespace OrchardCore.Users.ViewModels
{
    public class ChangeEmailViewModel : IValidatableObject
    {
        public string Email { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<ChangeEmailViewModel>>();
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(S["Email is required."], new[] { nameof(Email) });
            }
            else if (Email.IndexOf('@') == -1 || !MailboxAddress.TryParse(Email, out var emailAddress))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { nameof(Email) });
            }
        }
    }
}
