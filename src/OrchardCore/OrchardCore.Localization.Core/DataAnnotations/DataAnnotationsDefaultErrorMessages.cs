namespace OrchardCore.Localization.DataAnnotations;

/// <summary>
/// This is just a marker class to allow the POExtractor to extract the default error messages for data annotations attributes.
/// </summary>
internal sealed class DataAnnotationsDefaultErrorMessages
{
    public static LocalizationSource AssociatedMetadataTypeTypeDescriptorMetadataTypeContainsUnknownProperties => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The associated metadata type for type '{0}' contains the following unknown properties or fields: {1}. Please make sure that the names of these members match the names of the properties on the main type.");

    public static LocalizationSource AttributeStoreUnknownProperty => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The type '{0}' does not contain a public static property named '{1}'.");

    public static LocalizationSource CommonPropertyNotFound => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The property {0}.{1} could not be found.");

    public static LocalizationSource CompareAttributeMustMatch => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("'{0}' and '{1}' do not match.");

    public static LocalizationSource CompareAttributeUnknownProperty => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("Could not find a property named {0}.");

    public static LocalizationSource CreditCardAttributeInvalid => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} field is not a valid credit card number.");

    public static LocalizationSource CustomValidationAttributeMethodMustReturnValidationResult => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The CustomValidationAttribute method '{0}' in type '{1}' must return System.ComponentModel.DataAnnotations.ValidationResult.  Use System.ComponentModel.DataAnnotations.ValidationResult.Success to represent success.");

    public static LocalizationSource CustomValidationAttributeMethodNotFound => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The CustomValidationAttribute method '{0}' does not exist in type '{1}' or is not public static and static.");

    public static LocalizationSource CustomValidationAttributeMethodRequired => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The CustomValidationAttribute.Method was not specified.");

    public static LocalizationSource CustomValidationAttributeMethodSignature => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The CustomValidationAttribute method '{0}' in type '{1}' must match the expected signature: public static static ValidationResult {0}(object value, ValidationContext context). The value can be strongly typed. The ValidationContext parameter is optional.");

    public static LocalizationSource CustomValidationAttributeTypeConversionFailed => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("Could not convert the value of type '{0}' to '{1}' as expected by method {2}.{3}.");

    public static LocalizationSource CustomValidationAttributeTypeMustBePublic => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The custom validation type '{0}' must be public static.");

    public static LocalizationSource CustomValidationAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("{0} is not valid.");

    public static LocalizationSource CustomValidationAttributeValidatorTypeRequired => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The CustomValidationAttribute.ValidatorType was not specified.");

    public static LocalizationSource DataTypeAttributeEmptyDataTypeString => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The custom DataType string cannot be null or empty.");

    public static LocalizationSource DisplayAttributePropertyNotSet => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} property has not been set.  Use the {1} method to get the value.");

    public static LocalizationSource EmailAddressAttributeInvalid => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} field is not a valid e-mail address.");

    public static LocalizationSource EnumDataTypeAttributeTypeCannotBeNull => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The type provided for EnumDataTypeAttribute cannot be null.");

    public static LocalizationSource EnumDataTypeAttributeTypeNeedsToBeAnEnum => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The type '{0}' needs to represent an enumeration type.");

    public static LocalizationSource FileExtensionsAttributeInvalid => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} field only accepts files with the following extensions: {1}.");

    public static LocalizationSource LocalizableStringLocalizationFailed => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("Cannot retrieve property '{0}' because localization failed.  Type '{1}' is not public static or does not contain a public static static string property with the name '{2}'.");

    public static LocalizationSource MaxLengthAttributeInvalidMaxLength => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("MaxLengthAttribute must have a Length value that is greater than zero. Use MaxLength() without parameters to indicate that the string or array can have the maximum allowable length.");

    public static LocalizationSource MaxLengthAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} must be a string or array type with a maximum length of '{1}'.");

    public static LocalizationSource MetadataTypeAttributeTypeCannotBeNull => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("MetadataClassType cannot be null.");

    public static LocalizationSource MinLengthAttributeInvalidMinLength => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("MinLengthAttribute must have a Length value that is zero or greater.");

    public static LocalizationSource MinLengthAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} must be a string or array type with a minimum length of '{1}'.");

    public static LocalizationSource LengthAttributeInvalidValueType => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field of type {0} must be a string, array or ICollection type.");

    public static LocalizationSource PhoneAttributeInvalid => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} field is not a valid phone number.");

    public static LocalizationSource RangeAttributeArbitraryTypeNotIComparable => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The type {0} must implement {1}.");

    public static LocalizationSource RangeAttributeMinGreaterThanMax => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The maximum value '{0}' must be greater than or equal to the minimum value '{1}'.");

    public static LocalizationSource RangeAttributeMustSetMinAndMax => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The minimum and maximum values must be set.");

    public static LocalizationSource RangeAttributeMustSetOperandType => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The OperandType must be set when strings are used for minimum and maximum values.");

    public static LocalizationSource RangeAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} must be between {1} and {2}.");

    public static LocalizationSource RegexAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} must match the regular expression '{1}'.");

    public static LocalizationSource RegularExpressionAttributeEmptyPattern => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The pattern must be set to a valid regular expression.");

    public static LocalizationSource RequiredAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} field is required.");

    public static LocalizationSource StringLengthAttributeInvalidMaxLength => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The maximum length must be a nonnegative integer.");

    public static LocalizationSource StringLengthAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} must be a string with a maximum length of {1}.");

    public static LocalizationSource StringLengthAttributeValidationErrorIncludingMinimum => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} must be a string with a minimum length of {2} and a maximum length of {1}.");

    public static LocalizationSource UIHintImplementationControlParameterKeyIsNotAString => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The key parameter at position {0} with value '{1}' is not a string. Every key control parameter must be a string.");

    public static LocalizationSource UIHintImplementationControlParameterKeyIsNull => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The key parameter at position {0} is null. Every key control parameter must be a string.");

    public static LocalizationSource UIHintImplementationControlParameterKeyOccursMoreThanOnce => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The key parameter at position {0} with value '{1}' occurs more than once.");

    public static LocalizationSource UIHintImplementationNeedEvenNumberOfControlParameters => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The number of control parameters must be even.");

    public static LocalizationSource UrlAttributeInvalid => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The {0} field is not a valid fully-qualified http, https, or ftp URL.");

    public static LocalizationSource ValidationAttributeCannotSetErrorMessageAndResource => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("Either ErrorMessageString or ErrorMessageResourceName must be set, but not both.");

    public static LocalizationSource ValidationAttributeIsValidNotImplemented => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("IsValid(object value) has not been implemented by this class. The preferred entry point is GetValidationResult() and classes should override IsValid(object value, ValidationContext context).");

    public static LocalizationSource ValidationAttributeNeedBothResourceTypeAndResourceName => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("Both ErrorMessageResourceType and ErrorMessageResourceName need to be set on this attribute.");

    public static LocalizationSource ValidationAttributeResourcePropertyNotStringType => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The property '{0}' on resource type '{1}' is not a string type.");

    public static LocalizationSource ValidationAttributeResourceTypeDoesNotHaveProperty => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The resource type '{0}' does not have an accessible static property named '{1}'.");

    public static LocalizationSource ValidationAttributeValidationError => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The field {0} is invalid.");

    public static LocalizationSource ValidatorInstanceMustMatchValidationContextInstance => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The instance provided must match the ObjectInstance on the ValidationContext supplied.");

    public static LocalizationSource ValidatorPropertyValueWrongType => LocalizationSource.Create<DataAnnotationsDefaultErrorMessages>("The value for property '{0}' must be of type '{1}'.");
}
