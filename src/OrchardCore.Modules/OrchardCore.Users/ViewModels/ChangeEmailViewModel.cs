using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Email;

namespace OrchardCore.Users.ViewModels
{
    public class ChangeEmailViewModel : IValidatableObject
    {
        private readonly IEmailAddressValidator _emailAddressValidator;

        public ChangeEmailViewModel(IEmailAddressValidator emailAddressValidator)
        {
            _emailAddressValidator = emailAddressValidator ?? throw new ArgumentNullException(nameof(emailAddressValidator));
        }

        public string Email { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var S = validationContext.GetService<IStringLocalizer<ChangeEmailViewModel>>();
            if (string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(S["Email is required."], new[] { nameof(Email) });
            }
            else if (!_emailAddressValidator.Validate(Email))
            {
                yield return new ValidationResult(S["Invalid Email."], new[] { nameof(Email) });
            }
        }
    }
}
