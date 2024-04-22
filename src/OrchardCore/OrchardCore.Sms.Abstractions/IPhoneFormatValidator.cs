namespace OrchardCore.Sms;

public interface IPhoneFormatValidator
{
    bool IsValid(string phoneNumber);
}
