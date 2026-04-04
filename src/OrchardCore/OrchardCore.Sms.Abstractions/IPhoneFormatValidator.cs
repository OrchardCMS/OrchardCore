using OrchardCore.Infrastructure;

namespace OrchardCore.Sms;

/// <summary>
/// Provides phone number format validation and parsing.
/// </summary>
/// <example>
/// <code>
/// // Validate a phone number with explicit region:
/// var result = validator.Validate("+14155552671");
/// if (result.Succeeded)
/// {
///     var phone = result.Value;
///     Console.WriteLine(phone.E164Number);     // "+14155552671"
///     Console.WriteLine(phone.NationalNumber);  // "(415) 555-2671"
///     Console.WriteLine(phone.RegionCode);      // "US"
///     Console.WriteLine(phone.CountryCode);     // 1
/// }
///
/// // Validate with a default region for numbers without country code:
/// var result = validator.Validate("4155552671", "US");
/// </code>
/// </example>
public interface IPhoneFormatValidator
{
    /// <summary>
    /// Validates whether the given phone number is in a valid format.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <returns><see langword="true"/> if the phone number is valid; otherwise, <see langword="false"/>.</returns>
    [Obsolete("Use Validate() instead, which returns a Result<PhoneEntry> with detailed information.")]
    bool IsValid(string phoneNumber)
        => Validate(phoneNumber).Succeeded;

    /// <summary>
    /// Validates and parses the given phone number, returning a <see cref="Result{PhoneEntry}"/>
    /// containing the formatted phone number details on success, or error information on failure.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <param name="defaultRegion">
    /// An optional ISO 3166-1 alpha-2 region code (e.g., "US", "GB", "FR") used as the default
    /// country when the phone number does not include a country code. When <see langword="null"/>,
    /// the phone number must include the country code (e.g., "+14155552671").
    /// </param>
    /// <returns>
    /// A <see cref="Result{PhoneEntry}"/> containing the parsed <see cref="PhoneEntry"/> on success,
    /// or one or more <see cref="ResultError"/> entries on failure.
    /// </returns>
    Result<PhoneEntry> Validate(string phoneNumber, string defaultRegion = null);
}
