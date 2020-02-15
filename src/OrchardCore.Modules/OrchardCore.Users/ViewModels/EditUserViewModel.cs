using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserViewModel : IValidatableObject
    {
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsEnabled { get; set; }

        public RoleViewModel[] Roles { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<EditUserViewModel>>();

            if (!string.IsNullOrEmpty(Email) && !MailboxAddress.TryParse(Email, out var emailAddress))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { "Email" });
            }
        }
    }
}
