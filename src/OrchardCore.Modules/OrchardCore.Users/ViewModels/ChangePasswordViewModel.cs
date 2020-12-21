using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Users.ViewModels
{
    public class ChangePasswordViewModel : IValidatableObject
    {
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<ChangePasswordViewModel>>();
            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                yield return new ValidationResult(S["Current password is required."], new[] { nameof(CurrentPassword) });
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(S["Password is required."], new[] { nameof(Password) });
            }

            if (Password != PasswordConfirmation)
            {
                yield return new ValidationResult(S["The new password and confirmation password do not match."], new[] { nameof(PasswordConfirmation) });
            }
        }
    }
}
