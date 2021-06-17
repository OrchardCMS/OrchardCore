using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.DataAnnotations
{
    /// <summary>
    /// Provides a validation for a <see cref="DefaultModelMetadata"/>.
    /// </summary>
    public class LocalizedValidationMetadataProvider : IValidationMetadataProvider
    {
        private readonly IStringLocalizer _stringLocalizer;

        /// <summary>
        /// Initializes a new instance of a <see cref="LocalizedValidationMetadataProvider"/> with string localizer.
        /// </summary>
        /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
        public LocalizedValidationMetadataProvider(IStringLocalizer stringLocalizer)
        {
            _stringLocalizer = stringLocalizer;
        }

        /// <remarks>This will localize the default data annotations error message if it is exist, otherwise will try to look for a parameterized version.</remarks>
        /// <example>
        /// A property named 'UserName' that decorated with <see cref="RequiredAttribute"/> will be localized using
        /// "The {0} field is required." and "The UserName field is required." error messages.
        /// </example>
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            foreach (var metadata in context.ValidationMetadata.ValidatorMetadata)
            {
                if (metadata is ValidationAttribute attribute)
                {
                    var displayName = context.Attributes.OfType<DisplayAttribute>().FirstOrDefault()?.Name;
                    // Use DisplayName if present
                    var argument = displayName ?? context.Key.Name;
                    var errorMessageString = attribute.ErrorMessage == null && attribute.ErrorMessageResourceName == null
                        ? attribute.FormatErrorMessage(argument)
                        : attribute.ErrorMessage;

                    // Localize the parameterized error message
                    var localizedString = _stringLocalizer[errorMessageString];

                    if (localizedString == errorMessageString)
                    {
                        // If we get here, we don't have a translation for the parameterized error message,
                        // so try to look up a translation for the unparameterized error message.
                        //
                        // ASP.NET Core's DataAnnotations will handle parameterizing the error message
                        // and localizing the argument if it is a display name,
                        // so all we need to do here is localize the unparameterized error message.
                        //
                        // See https://github.com/aspnet/Mvc/blob/04ce6cae44fb0cb11470c21769d41e3f8088e8aa/src/Microsoft.AspNetCore.Mvc.DataAnnotations/ValidationAttributeAdapterOfTAttribute.cs#L82
                        // and https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.DataAnnotations/DataAnnotationsMetadataProvider.cs#L151
                        //
                        var unparameterizedErrorMessage = errorMessageString.Replace(argument, "{0}");
                        localizedString = _stringLocalizer[unparameterizedErrorMessage];
                    }

                    attribute.ErrorMessage = localizedString;
                }
            }
        }
    }
}
