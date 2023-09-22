namespace OrchardCore.Sms;

/// <summary>
/// Represents a contract for phone number validation service.
/// </summary>
public interface IPhoneNumberValidator
{
    /// <summary>
    /// Validates a phone number.
    /// </summary>
    /// <param name="phoneNumber">The phone number to be validated.</param>
    /// <returns><c>true</c> if the phone is valid, otherwise <c>false</c>.</returns>
    bool Validate(string phoneNumber);
}
