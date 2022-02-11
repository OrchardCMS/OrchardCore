using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Validation.Tests
{
    public class User : ValidatableObject
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string Email { get; set; }

        public async override Task<IEnumerable<ValidationResult>> ValidateAsync(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();
            if (String.IsNullOrEmpty(UserName))
            {
                errors.Add(new ValidationResult("The username is required."));
            }

            if (!String.IsNullOrEmpty(Password))
            {
                if (!Password.Equals(ConfirmPassword))
                {
                    errors.Add(new ValidationResult("The confirmation password doesn't match the password."));
                }
            }

            if (String.IsNullOrEmpty(UserName))
            {
                errors.Add(new ValidationResult("The email is required."));
            }

            if (!await CheckEmailAvailability(Email))
            {
                errors.Add(new ValidationResult("The email is already taken."));
            }

            return errors;
        }

        private static async Task<bool> CheckEmailAvailability(string email)
        {
            // Assume that the email checked from another service
            var emailsList = new string[]
            {
                "admin@orchardcore.com",
                "admin@orchard.com",
                "admin@oc.com"
            };

            var exist = emailsList.Contains(email);

            await Task.CompletedTask;

            return exist;
        }
    }
}
