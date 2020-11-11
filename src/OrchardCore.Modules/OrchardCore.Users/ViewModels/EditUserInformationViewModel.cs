using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserInformationViewModel : IValidatableObject
    {
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();
            var S = validationContext.GetService<IStringLocalizer<EditUserViewModel>>();

            if (!string.IsNullOrEmpty(Email) && !emailAddressValidator.Validate(Email))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { "Email" });
            }
        }
    }
}
