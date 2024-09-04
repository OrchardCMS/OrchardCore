using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email;

/// <summary>
/// Validates an email address.
/// </summary>
public class EmailAddressAttribute : ValidationAttribute
{
    /// <inheritdoc/>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var emailAddressValidator = validationContext.GetService<IEmailAddressValidator>();
        var S = validationContext.GetService<IStringLocalizer<EmailAddressAttribute>>();

        if (value == null || emailAddressValidator.Validate(value.ToString()))
        {
            return ValidationResult.Success;
        }

        return new ValidationResult(S["Invalid email address."], new[] { nameof(Email) });
    }
}
