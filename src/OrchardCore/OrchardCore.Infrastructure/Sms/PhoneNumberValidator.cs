using System;
using PhoneNumbers;

namespace OrchardCore.Sms;

/// <summary>
/// Represents a service for phone number validation.
/// </summary>
public class PhoneNumberValidator : IPhoneNumberValidator
{
    /// <inheritdoc/>
    public bool Validate(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(phoneNumber));
        }

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
