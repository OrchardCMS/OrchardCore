using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;
using PhoneNumbers;

namespace OrchardCore;

public class PhoneFormatValidator : IPhoneFormatValidator
{
    internal readonly IStringLocalizer S;

    public PhoneFormatValidator(IStringLocalizer<PhoneFormatValidator> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public bool IsValid(string phoneNumber)
        => Validate(phoneNumber).Succeeded;

    public Result<PhoneEntry> Validate(string phoneNumber, string defaultRegion = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Result.Failed<PhoneEntry>(new ResultError
            {
                Message = S["Please provide a phone number."],
            });
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

            return Result.Failed<PhoneEntry>(new ResultError
            {
                Message = message,
            });
        }

        if (!phoneNumberUtil.IsValidNumber(phone))
        {
            var regionCode = phoneNumberUtil.GetRegionCodeForNumber(phone);

            if (regionCode != null && !phoneNumberUtil.IsValidNumberForRegion(phone, regionCode))
            {
                return Result.Failed<PhoneEntry>(new ResultError
                {
                    Message = S["The phone number is not valid for the detected region ({0}).", regionCode],
                });
            }

            return Result.Failed<PhoneEntry>(new ResultError
            {
                Message = S["Please provide a valid phone number."],
            });
        }

        var detectedRegion = phoneNumberUtil.GetRegionCodeForNumber(phone);

        return Result.Success(new PhoneEntry
        {
            E164Number = phoneNumberUtil.Format(phone, PhoneNumberFormat.E164),
            NationalNumber = phoneNumberUtil.Format(phone, PhoneNumberFormat.NATIONAL),
            RegionCode = detectedRegion,
            CountryCode = phone.CountryCode,
        });
    }
}
