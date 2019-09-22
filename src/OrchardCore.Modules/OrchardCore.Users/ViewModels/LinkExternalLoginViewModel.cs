using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class LinkExternalLoginViewModel : IValidatableObject
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult("Password is required", new[] { nameof(Password) });
            }
        }
    }
}
