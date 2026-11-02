namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="ConvertTimeZoneStep"/>.
/// </summary>
public sealed class ConvertTimeZoneStepSettings
{
    /// <summary>
    /// Gets or sets the date and time fields to convert, or none to convert every date and time field.
    /// </summary>
    public List<string> Fields { get; set; } = [];

    /// <summary>
    /// Gets or sets the direction of the conversion.
    /// </summary>
    public TimeZoneConversion Conversion { get; set; } = TimeZoneConversion.FromUtc;

    /// <summary>
    /// Gets or sets the time zone the values are converted to or from.
    /// </summary>
    public TimeZoneSource TimeZone { get; set; } = TimeZoneSource.Site;

    /// <summary>
    /// Gets or sets the IANA identifier of the time zone, such as <c>Europe/Paris</c>, when <see cref="TimeZone"/>
    /// is <see cref="TimeZoneSource.Specific"/>.
    /// </summary>
    public string TimeZoneId { get; set; }
}

/// <summary>
/// The direction of a time zone conversion.
/// </summary>
public enum TimeZoneConversion
{
    /// <summary>
    /// The values are in UTC, as Orchard Core stores dates, and become the local date and time of the time zone.
    /// </summary>
    FromUtc,

    /// <summary>
    /// The values are local dates and times of the time zone, such as dates read from a file, and become UTC.
    /// </summary>
    ToUtc,
}

/// <summary>
/// The time zone of a time zone conversion.
/// </summary>
public enum TimeZoneSource
{
    /// <summary>
    /// The time zone of the site, from its general settings.
    /// </summary>
    Site,

    /// <summary>
    /// The time zone of the user the pipeline runs for: the user who published it, or the designer during a preview.
    /// The time zone of the site when the user chose none.
    /// </summary>
    User,

    /// <summary>
    /// The time zone of <see cref="ConvertTimeZoneStepSettings.TimeZoneId"/>.
    /// </summary>
    Specific,
}
