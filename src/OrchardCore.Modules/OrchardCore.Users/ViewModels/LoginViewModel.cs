using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Users.ViewModels
{
    public class LoginViewModel : IValidatableObject
    {
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<LoginViewModel>>();
            if (string.IsNullOrWhiteSpace(UserName))
            {
                    yield return new ValidationResult(S["Username is required."], new[] { "UserName" });
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(S["Password is required."], new[] { "Password" });
            }
        }
    }
}
