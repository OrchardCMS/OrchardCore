namespace OrchardCore.Sms;

public class DefaultPhoneFormatValidator : IPhoneFormatValidator
{
    private readonly IPhoneNumberValidator _phoneNumberValidator;

    public DefaultPhoneFormatValidator(IPhoneNumberValidator phoneNumberValidator)
        => _phoneNumberValidator = phoneNumberValidator;

    public bool IsValid(string phoneNumber) => _phoneNumberValidator.Validate(phoneNumber);
}
