namespace OrchardCore.Sms;

public interface IPhoneNumberValidator
{
    bool Validate(string phoneNumber);
}
