using Microsoft.Extensions.Localization;

namespace OrchardCore.Localization.DataAnnotations
{
    /// <summary>
    /// This is just a marker class to allow the POExtractor to extract the default error messages for data annotations attributes.
    /// </summary>
    internal sealed class DataAnnotationsDefaultErrorMessages
    {
#pragma warning disable IDE1006 // Naming Styles
        private readonly IStringLocalizer S = null;
#pragma warning restore IDE1006 // Naming Styles

        public string AssociatedMetadataTypeTypeDescriptorMetadataTypeContainsUnknownProperties => S["The associated metadata type for type '{0}' contains the following unknown properties or fields: {1}. Please make sure that the names of these members match the names of the properties on the main type."];

        public string AttributeStoreUnknownProperty => S["The type '{0}' does not contain a public property named '{1}'."];

        public string CommonPropertyNotFound => S["The property {0}.{1} could not be found."];

        public string CompareAttributeMustMatch => S["'{0}' and '{1}' do not match."];

        public string CompareAttributeUnknownProperty => S["Could not find a property named {0}."];

        public string CreditCardAttributeInvalid => S["The {0} field is not a valid credit card number."];

        public string CustomValidationAttributeMethodMustReturnValidationResult => S["The CustomValidationAttribute method '{0}' in type '{1}' must return System.ComponentModel.DataAnnotations.ValidationResult.  Use System.ComponentModel.DataAnnotations.ValidationResult.Success to represent success."];

        public string CustomValidationAttributeMethodNotFound => S["The CustomValidationAttribute method '{0}' does not exist in type '{1}' or is not public and static."];

        public string CustomValidationAttributeMethodRequired => S["The CustomValidationAttribute.Method was not specified."];

        public string CustomValidationAttributeMethodSignature => S["The CustomValidationAttribute method '{0}' in type '{1}' must match the expected signature: public static ValidationResult {0}(object value, ValidationContext context). The value can be strongly typed. The ValidationContext parameter is optional."];

        public string CustomValidationAttributeTypeConversionFailed => S["Could not convert the value of type '{0}' to '{1}' as expected by method {2}.{3}."];

        public string CustomValidationAttributeTypeMustBePublic => S["The custom validation type '{0}' must be public."];

        public string CustomValidationAttributeValidationError => S["{0} is not valid."];

        public string CustomValidationAttributeValidatorTypeRequired => S["The CustomValidationAttribute.ValidatorType was not specified."];

        public string DataTypeAttributeEmptyDataTypeString => S["The custom DataType string cannot be null or empty."];

        public string DisplayAttributePropertyNotSet => S["The {0} property has not been set.  Use the {1} method to get the value."];

        public string EmailAddressAttributeInvalid => S["The {0} field is not a valid e-mail address."];

        public string EnumDataTypeAttributeTypeCannotBeNull => S["The type provided for EnumDataTypeAttribute cannot be null."];

        public string EnumDataTypeAttributeTypeNeedsToBeAnEnum => S["The type '{0}' needs to represent an enumeration type."];

        public string FileExtensionsAttributeInvalid => S["The {0} field only accepts files with the following extensions: {1}."];

        public string LocalizableStringLocalizationFailed => S["Cannot retrieve property '{0}' because localization failed.  Type '{1}' is not public or does not contain a public static string property with the name '{2}'."];

        public string MaxLengthAttributeInvalidMaxLength => S["MaxLengthAttribute must have a Length value that is greater than zero. Use MaxLength() without parameters to indicate that the string or array can have the maximum allowable length."];

        public string MaxLengthAttributeValidationError => S["The field {0} must be a string or array type with a maximum length of '{1}'."];

        public string MetadataTypeAttributeTypeCannotBeNull => S["MetadataClassType cannot be null."];

        public string MinLengthAttributeInvalidMinLength => S["MinLengthAttribute must have a Length value that is zero or greater."];

        public string MinLengthAttributeValidationError => S["The field {0} must be a string or array type with a minimum length of '{1}'."];

        public string LengthAttributeInvalidValueType => S["The field of type {0} must be a string, array or ICollection type."];

        public string PhoneAttributeInvalid => S["The {0} field is not a valid phone number."];

        public string RangeAttributeArbitraryTypeNotIComparable => S["The type {0} must implement {1}."];

        public string RangeAttributeMinGreaterThanMax => S["The maximum value '{0}' must be greater than or equal to the minimum value '{1}'."];

        public string RangeAttributeMustSetMinAndMax => S["The minimum and maximum values must be set."];

        public string RangeAttributeMustSetOperandType => S["The OperandType must be set when strings are used for minimum and maximum values."];

        public string RangeAttributeValidationError => S["The field {0} must be between {1} and {2}."];

        public string RegexAttributeValidationError => S["The field {0} must match the regular expression '{1}'."];

        public string RegularExpressionAttributeEmptyPattern => S["The pattern must be set to a valid regular expression."];

        public string RequiredAttributeValidationError => S["The {0} field is required."];

        public string StringLengthAttributeInvalidMaxLength => S["The maximum length must be a nonnegative integer."];

        public string StringLengthAttributeValidationError => S["The field {0} must be a string with a maximum length of {1}."];

        public string StringLengthAttributeValidationErrorIncludingMinimum => S["The field {0} must be a string with a minimum length of {2} and a maximum length of {1}."];

        public string UIHintImplementationControlParameterKeyIsNotAString => S["The key parameter at position {0} with value '{1}' is not a string. Every key control parameter must be a string."];

        public string UIHintImplementationControlParameterKeyIsNull => S["The key parameter at position {0} is null. Every key control parameter must be a string."];

        public string UIHintImplementationControlParameterKeyOccursMoreThanOnce => S["The key parameter at position {0} with value '{1}' occurs more than once."];

        public string UIHintImplementationNeedEvenNumberOfControlParameters => S["The number of control parameters must be even."];

        public string UrlAttributeInvalid => S["The {0} field is not a valid fully-qualified http, https, or ftp URL."];

        public string ValidationAttributeCannotSetErrorMessageAndResource => S["Either ErrorMessageString or ErrorMessageResourceName must be set, but not both."];

        public string ValidationAttributeIsValidNotImplemented => S["IsValid(object value) has not been implemented by this class. The preferred entry point is GetValidationResult() and classes should override IsValid(object value, ValidationContext context)."];

        public string ValidationAttributeNeedBothResourceTypeAndResourceName => S["Both ErrorMessageResourceType and ErrorMessageResourceName need to be set on this attribute."];

        public string ValidationAttributeResourcePropertyNotStringType => S["The property '{0}' on resource type '{1}' is not a string type."];

        public string ValidationAttributeResourceTypeDoesNotHaveProperty => S["The resource type '{0}' does not have an accessible static property named '{1}'."];

        public string ValidationAttributeValidationError => S["The field {0} is invalid."];

        public string ValidatorInstanceMustMatchValidationContextInstance => S["The instance provided must match the ObjectInstance on the ValidationContext supplied."];

        public string ValidatorPropertyValueWrongType => S["The value for property '{0}' must be of type '{1}'."];
    }
}
