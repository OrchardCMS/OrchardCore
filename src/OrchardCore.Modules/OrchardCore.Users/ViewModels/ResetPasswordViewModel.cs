using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MimeKit;

namespace OrchardCore.Users.ViewModels
{
    public class ResetPasswordViewModel : IValidatableObject
    {
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public string ResetToken { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<ChangePasswordViewModel>>();
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(S["Email is required."], new[] { nameof(Email) });
            }
            else if (Email.IndexOf('@') == -1 || !MailboxAddress.TryParse(Email, out var emailAddress))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { nameof(Email) });
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                yield return new ValidationResult(S["New password is required."], new[] { nameof(NewPassword) });
            }

            if (NewPassword != PasswordConfirmation)
            {
                yield return new ValidationResult(S["The new password and confirmation password do not match."], new[] { nameof(PasswordConfirmation) });
            }
        }
    }
}
