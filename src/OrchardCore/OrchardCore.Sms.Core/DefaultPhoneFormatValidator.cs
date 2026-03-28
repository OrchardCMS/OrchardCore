using Microsoft.Extensions.Localization;
using PhoneNumbers;

namespace OrchardCore.Sms;

public class DefaultPhoneFormatValidator : IPhoneFormatValidator
{
    internal readonly IStringLocalizer S;

    public DefaultPhoneFormatValidator(IStringLocalizer<DefaultPhoneFormatValidator> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public bool IsValid(string phoneNumber)
        => Validate(phoneNumber).IsValid;

    public PhoneValidationResult Validate(string phoneNumber, string defaultRegion = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return PhoneValidationResult.Failure(S["Please provide a phone number."]);
        }

        var phoneNumberUtil = PhoneNumberUtil.GetInstance();

        PhoneNumber phone;
        try
        {
            phone = phoneNumberUtil.Parse(phoneNumber, defaultRegion);
        }
        catch (NumberParseException ex)
        {
            var message = ex.ErrorType switch
            {
                ErrorType.INVALID_COUNTRY_CODE => S["The phone number has an invalid country code. Please include a valid country code (e.g., +1 for US)."],
                ErrorType.NOT_A_NUMBER => S["The value provided is not a valid phone number."],
                ErrorType.TOO_SHORT_AFTER_IDD => S["The phone number is too short after the country code."],
                ErrorType.TOO_SHORT_NSN => S["The phone number is too short."],
                ErrorType.TOO_LONG => S["The phone number is too long."],
                _ => S["Please provide a valid phone number."],
            };

            return PhoneValidationResult.Failure(message);
        }

        if (!phoneNumberUtil.IsValidNumber(phone))
        {
            var regionCode = phoneNumberUtil.GetRegionCodeForNumber(phone);

            if (regionCode != null && !phoneNumberUtil.IsValidNumberForRegion(phone, regionCode))
            {
                return PhoneValidationResult.Failure(S["The phone number is not valid for the detected region ({0}).", regionCode]);
            }

            return PhoneValidationResult.Failure(S["Please provide a valid phone number."]);
        }

        var detectedRegion = phoneNumberUtil.GetRegionCodeForNumber(phone);

        return PhoneValidationResult.Success(
            e164: phoneNumberUtil.Format(phone, PhoneNumberFormat.E164),
            national: phoneNumberUtil.Format(phone, PhoneNumberFormat.NATIONAL),
            regionCode: detectedRegion,
            countryCode: phone.CountryCode
        );
    }
}
