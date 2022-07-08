using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.Users.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();
            var S = validationContext.GetService<IStringLocalizer<RegisterViewModel>>();

            if (string.IsNullOrWhiteSpace(UserName))
            {
                yield return new ValidationResult(S["Username is required."], new[] { nameof(UserName) });
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(S["Email is required."], new[] { nameof(Email) });
            }
            else if (!emailAddressValidator.Validate(Email))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { nameof(Email) });
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(S["Password is required."], new[] { nameof(Password) });
            }

            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult(S["The new password and confirmation password do not match."], new[] { nameof(ConfirmPassword) });
            }
        }
    }
}
