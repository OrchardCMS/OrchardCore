using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.DataAnnotations;

/// <summary>
/// Provides a validation for a <see cref="DefaultModelMetadata"/>.
/// </summary>
[SkipLocalizationExtraction]
public class LocalizedValidationMetadataProvider : IValidationMetadataProvider
{
    private const string NamePlaceholder = "{0}";

    private readonly IStringLocalizer _stringLocalizer;

    /// <summary>
    /// Initializes a new instance of a <see cref="LocalizedValidationMetadataProvider"/> with string localizer.
    /// </summary>
    /// <param name="stringLocalizer">The <see cref="IStringLocalizer"/>.</param>
    public LocalizedValidationMetadataProvider(IStringLocalizer stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }

    /// <remarks>
    /// This will localize the error message of each validation attribute, keeping the placeholders that the attribute
    /// formats when the validation fails. A message set with <see cref="ValidationAttribute.ErrorMessage"/> is localized
    /// as is, then with the name of the property replaced by <c>{0}</c>. The default message of an attribute is localized
    /// with the name of the property, then as its unformatted template, e.g. "The field {0} must be between {1} and {2}.",
    /// then with the name of the property replaced by <c>{0}</c>. Attributes using a resource are left unchanged, and so
    /// are attributes whose message has no translation.
    /// </remarks>
    /// <example>
    /// A property named 'UserName' that decorated with <see cref="RequiredAttribute"/> will be localized using
    /// "The UserName field is required." and "The {0} field is required." error messages.
    /// </example>
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var metadata in context.ValidationMetadata.ValidatorMetadata)
        {
            if (metadata is not ValidationAttribute attribute || attribute.ErrorMessageResourceName != null)
            {
                // An attribute using a resource is localized by its resource, and setting its ErrorMessage would make
                // it invalid.
                continue;
            }

            var displayName = context.Attributes.OfType<DisplayAttribute>().FirstOrDefault()?.Name;
            // Use DisplayName if present
            var argument = displayName ?? context.Key.Name;

            var errorMessage = attribute.ErrorMessage == null
                ? LocalizeDefaultErrorMessage(attribute, argument)
                : LocalizeErrorMessage(attribute.ErrorMessage, argument);

            if (errorMessage != null)
            {
                attribute.ErrorMessage = errorMessage;
            }
        }
    }

    // The ErrorMessage is a composite format that the attribute formats with the name of the property and its own
    // arguments, it is localized as is.
    private string LocalizeErrorMessage(string errorMessage, string argument)
    {
        if (TryLocalize(errorMessage, out var localizedErrorMessage))
        {
            return localizedErrorMessage;
        }

        var unparameterizedErrorMessage = errorMessage.Replace(argument, NamePlaceholder);

        return TryLocalize(unparameterizedErrorMessage, out localizedErrorMessage)
            ? localizedErrorMessage
            : null;
    }

    // The default error message is formatted by the attribute. The ErrorMessage that replaces it must still be a
    // composite format, as the attribute formats it again when the validation fails, with arguments like a regular
    // expression pattern that can contain braces, e.g. '^#[0-9a-fA-F]{6}$'.
    private string LocalizeDefaultErrorMessage(ValidationAttribute attribute, string argument)
    {
        // Localize the error message formatted with the name of the property, which has no placeholders left.
        var formattedErrorMessage = attribute.FormatErrorMessage(argument);

        if (TryLocalize(formattedErrorMessage, out var localizedErrorMessage))
        {
            return EscapeBraces(localizedErrorMessage);
        }

        // Localize the unformatted error message, which keeps the placeholders of all the arguments.
        var errorMessageTemplate = GetErrorMessageTemplate(attribute);

        if (TryLocalize(errorMessageTemplate, out localizedErrorMessage))
        {
            return localizedErrorMessage;
        }

        // Localize the formatted error message with the name of the property replaced by its placeholder.
        var unparameterizedErrorMessage = formattedErrorMessage.Replace(argument, NamePlaceholder);

        if (unparameterizedErrorMessage != errorMessageTemplate &&
            TryLocalize(unparameterizedErrorMessage, out localizedErrorMessage))
        {
            return EscapeBraces(localizedErrorMessage, NamePlaceholder);
        }

        return null;
    }

    private bool TryLocalize(string name, out string value)
    {
        var localizedString = _stringLocalizer[name];

        value = localizedString.Value;

        return !localizedString.ResourceNotFound && value != name;
    }

    private static string GetErrorMessageTemplate(ValidationAttribute attribute)
    {
        // The StringLengthAttribute formats a different message than its ErrorMessageString when a minimum length is
        // set and the error message is the default one.
        if (attribute is StringLengthAttribute { MinimumLength: not 0 })
        {
            return DataAnnotationsDefaultErrorMessages.StringLengthAttributeValidationErrorIncludingMinimum.Value;
        }

        return GetErrorMessageString(attribute);
    }

    private static string EscapeBraces(string value, string placeholder = null)
    {
        if (placeholder == null)
        {
            return value.Replace("{", "{{").Replace("}", "}}");
        }

        return string.Join(placeholder, value.Split(placeholder).Select(part => EscapeBraces(part)));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_ErrorMessageString")]
    private static extern string GetErrorMessageString(ValidationAttribute attribute);
}
