namespace OrchardCore.Sms;

public interface IPhoneFormatValidator
{
    bool IsValid(string phoneNumber);

    PhoneValidationResult Validate(string phoneNumber, string defaultRegion = null)
        => IsValid(phoneNumber)
            ? PhoneValidationResult.Success(phoneNumber, phoneNumber, defaultRegion, 0)
            : PhoneValidationResult.Failure("Please provide a valid phone number.");
}
