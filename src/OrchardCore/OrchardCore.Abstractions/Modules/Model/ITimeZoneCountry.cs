namespace OrchardCore.Modules
{
    /// <summary>
    /// Represents a time zone.
    /// </summary>
    public interface ITimeZoneCountry
    {
        string CountryCode { get; set; }
        string CountryName { get; set; }
    }
}
