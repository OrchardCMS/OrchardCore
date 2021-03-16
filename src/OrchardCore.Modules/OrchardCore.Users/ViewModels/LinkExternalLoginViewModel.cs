using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Users.ViewModels
{
    public class LinkExternalLoginViewModel : IValidatableObject
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<LinkExternalLoginViewModel>>();
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(S["Password is required"], new[] { nameof(Password) });
            }
        }
    }
}
