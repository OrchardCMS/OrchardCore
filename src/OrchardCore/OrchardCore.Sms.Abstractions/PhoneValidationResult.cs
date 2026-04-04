namespace OrchardCore.Sms;

[Obsolete("Use Result<PhoneEntry> from IPhoneFormatValidator.Validate() instead.")]
public class PhoneValidationResult
{
    public bool IsValid { get; set; }

    /// <summary>
    /// The phone number in E.164 format (e.g., "+14155552671").
    /// </summary>
    public string E164Number { get; set; }

    /// <summary>
    /// The phone number in national format (e.g., "(415) 555-2671").
    /// </summary>
    public string NationalNumber { get; set; }

    /// <summary>
    /// The ISO 3166-1 alpha-2 region code (e.g., "US").
    /// </summary>
    public string RegionCode { get; set; }

    /// <summary>
    /// The country calling code (e.g., 1 for US).
    /// </summary>
    public int CountryCode { get; set; }

    /// <summary>
    /// A localized error message when <see cref="IsValid"/> is <see langword="false"/>.
    /// </summary>
    public string ErrorMessage { get; set; }

    public static PhoneValidationResult Success(string e164, string national, string regionCode, int countryCode)
        => new()
        {
            IsValid = true,
            E164Number = e164,
            NationalNumber = national,
            RegionCode = regionCode,
            CountryCode = countryCode,
        };

    public static PhoneValidationResult Failure(string errorMessage)
        => new()
        {
            IsValid = false,
            ErrorMessage = errorMessage,
        };
}
