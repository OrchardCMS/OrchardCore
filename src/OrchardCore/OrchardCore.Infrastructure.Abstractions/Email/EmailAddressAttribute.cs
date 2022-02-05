using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Email
{
    /// <summary>
    /// Validates an email address.
    /// </summary>
    public class EmailAddressAttribute : ValidationAttribute
    {
        /// <inheritdoc/>
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
