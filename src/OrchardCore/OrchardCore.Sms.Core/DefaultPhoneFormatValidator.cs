using System;

namespace OrchardCore.Sms;

[Obsolete("This class has been deprecated, please use PhoneNumberValidator instead.")]
public class DefaultPhoneFormatValidator : IPhoneFormatValidator
{
    private readonly IPhoneNumberValidator _phoneNumberValidator;

    public DefaultPhoneFormatValidator(IPhoneNumberValidator phoneNumberValidator)
        => _phoneNumberValidator = phoneNumberValidator;

    public bool IsValid(string phoneNumber) => _phoneNumberValidator.Validate(phoneNumber);
}
