namespace OrchardCore;

/// <summary>
/// Represents a validated phone number with its formatted values.
/// </summary>
/// <example>
/// <code>
/// var result = phoneFormatValidator.Validate("+14155552671");
/// if (result.Succeeded)
/// {
///     var entry = result.Value;
///     // entry.E164Number: "+14155552671"
///     // entry.NationalNumber: "(415) 555-2671"
///     // entry.RegionCode: "US"
///     // entry.CountryCode: 1
/// }
/// </code>
/// </example>
public class PhoneEntry
{
    /// <summary>
    /// Gets the phone number in E.164 format (e.g., "+14155552671").
    /// </summary>
    public string E164Number { get; init; }

    /// <summary>
    /// Gets the phone number in national format (e.g., "(415) 555-2671").
    /// </summary>
    public string NationalNumber { get; init; }

    /// <summary>
    /// Gets the ISO 3166-1 alpha-2 region code (e.g., "US").
    /// </summary>
    public string RegionCode { get; init; }

    /// <summary>
    /// Gets the country calling code (e.g., 1 for US).
    /// </summary>
    public int CountryCode { get; init; }
}
