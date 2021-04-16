using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserInformationViewModel : IValidatableObject
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Email { get; set; }

        [BindNever]
        public bool IsEditingDisabled { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();
            var S = validationContext.GetService<IStringLocalizer<EditUserInformationViewModel>>();

            if (!string.IsNullOrEmpty(Email) && !emailAddressValidator.Validate(Email))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { "Email" });
            }
        }
    }
}
