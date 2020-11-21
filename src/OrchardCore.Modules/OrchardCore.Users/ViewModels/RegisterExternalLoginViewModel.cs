using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.Users.ViewModels
{
    public class RegisterExternalLoginViewModel : IValidatableObject
    {
        public bool NoUsername { get; set; }

        public bool NoEmail { get; set; }

        public bool NoPassword { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();
            var S = validationContext.GetService<IStringLocalizer<RegisterExternalLoginViewModel>>();

            if (string.IsNullOrWhiteSpace(Email))
            {
                if (!NoEmail)
                {
                    yield return new ValidationResult(S["Email is required!"], new[] { "Email" });
                }
            }
            else if (!emailAddressValidator.Validate(Email))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { "Email" });
            }

            if (string.IsNullOrWhiteSpace(UserName) && !NoUsername)
            {
                yield return new ValidationResult(S["Username is required!"], new[] { "UserName" });
            }

            if (string.IsNullOrWhiteSpace(Password) && !NoPassword)
            {
                yield return new ValidationResult(S["Password is required!"], new[] { "Password" });
            }

            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult(S["Confirm Password do not match"], new[] { "ConfirmPassword" });
            }

            if (Password != null && (Password.Length < 6 || Password.Length > 100))
            {
                yield return new ValidationResult(string.Format(S["Password must be between {0} and {1} characters"], 6, 100), new[] { "Password" });
            }
        }
    }
}
