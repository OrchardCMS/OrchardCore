using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class RegisterExternalLoginViewModel : IValidatableObject
    {
        public bool NoUsername { get; set; }
        public bool NoEmail { get; set; }
        public bool NoPassword { get; set; }

        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Email) && !NoEmail)
            {
                yield return new ValidationResult("Email is required!", new[] { "Email" });
            }

            if (string.IsNullOrWhiteSpace(UserName) && !NoUsername)
            {
                yield return new ValidationResult("Username is required!", new[] { "UserName" });
            }

            if (string.IsNullOrWhiteSpace(Password) && !NoPassword)
            {
                yield return new ValidationResult("Password is required!", new[] { "Password" });
            }

            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult("Confirm Password do not match", new[] { "ConfirmPassword" });
            }

            if (Password != null && (Password.Length < 6 || Password.Length > 100))
            {
                yield return new ValidationResult(string.Format("Password must be between {0} and {1} characters", 6, 100), new[] { "Password" });
            }
        }
    }
}
