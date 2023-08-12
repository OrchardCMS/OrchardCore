using PhoneNumbers;

namespace OrchardCore.Sms;

public class DefaultPhoneFormatValidator : IPhoneFormatValidator
{
    private PhoneNumberUtil _phoneNumberUtil;

    public bool IsValid(string phoneNumber)
    {
        _phoneNumberUtil ??= PhoneNumberUtil.GetInstance();

        try
        {
            var phone = _phoneNumberUtil.Parse(phoneNumber, null);

            return _phoneNumberUtil.IsValidNumber(phone);
        }
        catch { }

        return false;
    }
}
