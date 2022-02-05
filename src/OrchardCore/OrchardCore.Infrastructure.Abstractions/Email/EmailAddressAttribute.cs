using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Email
{
    public class EmailAddressAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();

            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (emailAddressValidator.Validate(value.ToString()))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult(ErrorMessage, new[] { nameof(Email) });
            }
        }
    }
}
