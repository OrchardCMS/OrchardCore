using PhoneNumbers;

namespace OrchardCore.Sms;

public class DefaultPhoneFormatValidator : IPhoneFormatValidator
{
    public bool IsValid(string phoneNumber)
    {
        var phoneNumberUtil = PhoneNumberUtil.GetInstance();

        try
        {
            var phone = phoneNumberUtil.Parse(phoneNumber, null);

            return phoneNumberUtil.IsValidNumber(phone);
        }
        catch
        {
            return false;
        }
    }
}
