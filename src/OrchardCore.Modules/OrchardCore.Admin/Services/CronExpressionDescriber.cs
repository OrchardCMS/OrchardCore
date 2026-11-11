using System.Globalization;
using CronExpressionDescriptor;
using NCrontab;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Admin.Services;

/// <summary>
/// Describes cron expressions with five fields, as the ones of the background tasks, the jobs and the workflow
/// timers, which are all evaluated with NCrontab.
/// </summary>
internal static class CronExpressionDescriber
{
    /// <summary>
    /// The longest expression that is looked at.
    /// </summary>
    public const int MaxLength = 256;

    /// <summary>
    /// The name UTC is shown with.
    /// </summary>
    public const string Utc = "UTC";

    /// <summary>
    /// Gets whether an expression is one the schedules can be evaluated with.
    /// </summary>
    /// <param name="expression">The cron expression.</param>
    /// <returns><see langword="true"/> when the expression is a valid cron expression with five fields.</returns>
    public static bool IsValid(string expression)
    {
        return !string.IsNullOrWhiteSpace(expression)
            && expression.Length <= MaxLength
            && CrontabSchedule.TryParse(expression) is not null;
    }

    /// <summary>
    /// Describes when a cron expression occurs, such as "At 03:00 AM", in the language of a culture when the
    /// descriptions are translated to it, and in English otherwise.
    /// </summary>
    /// <param name="expression">The cron expression.</param>
    /// <param name="culture">The culture of the description.</param>
    /// <returns>The description, or <see langword="null"/> when the expression is not valid or can't be described.</returns>
    public static string Describe(string expression, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        if (!IsValid(expression))
        {
            return null;
        }

        try
        {
            var description = ExpressionDescriptor.GetDescription(expression.Trim(), new Options
            {
                Locale = string.IsNullOrEmpty(culture.Name) ? "en" : culture.Name,
                ThrowExceptionOnParseError = true,

                // The times follow the culture: 3:00 PM where it writes the period of the day, 15:00 elsewhere.
                Use24HourTimeFormat = !culture.DateTimeFormat.ShortTimePattern.Contains('t'),
            });

            return string.IsNullOrWhiteSpace(description) ? null : description;
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            // The expression is valid for the schedules, even if it has no description.
            return null;
        }
    }

    /// <summary>
    /// Gets the time zone a cron expression applies to.
    /// </summary>
    /// <param name="clock">The clock.</param>
    /// <param name="siteService">The service that gives the time zone of the site.</param>
    /// <param name="timeZoneId">The identifier of the time zone of the expression, if it has its own.</param>
    /// <param name="utc">Whether an expression without a time zone applies to UTC, and not to the time zone of the site.</param>
    /// <returns>
    /// The time zone, <see langword="null"/> for UTC, and whether it is the time zone of the site, which an
    /// expression follows when it has none of its own, or whose own one is not known.
    /// </returns>
    public static async Task<(ITimeZone TimeZone, bool IsSiteTimeZone)> GetTimeZoneAsync(IClock clock, ISiteService siteService, string timeZoneId, bool utc)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            var timeZone = clock.GetTimeZone(timeZoneId);

            // The clock answers with the time zone of the server for an identifier it doesn't know.
            if (string.Equals(timeZone.TimeZoneId, timeZoneId, StringComparison.OrdinalIgnoreCase))
            {
                return (timeZone, false);
            }
        }

        if (utc)
        {
            return (null, false);
        }

        return (clock.GetTimeZone((await siteService.GetSiteSettingsAsync()).TimeZoneId), true);
    }

    /// <summary>
    /// Gets the abbreviation of a time zone at a date, such as PST or PDT for <c>America/Los_Angeles</c>, which
    /// tells a description applies to a time zone that is not the one of the site.
    /// </summary>
    /// <param name="timeZone">The time zone, <see langword="null"/> for UTC.</param>
    /// <param name="utcNow">The date, in UTC, as the abbreviation of a time zone changes with daylight saving time.</param>
    /// <returns>The abbreviation, or the offset from UTC, such as UTC+05:30, for a time zone that has none.</returns>
    public static string GetAbbreviation(ITimeZone timeZone, DateTime utcNow)
    {
        if (timeZone is null)
        {
            return Utc;
        }

        var zone = NodaTime.DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZone.TimeZoneId);

        if (zone is null)
        {
            return timeZone.TimeZoneId;
        }

        var interval = zone.GetZoneInterval(NodaTime.Instant.FromDateTimeUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc)));

        // The time zones without an abbreviation are named after their offset, such as +0530 or -03.
        return string.IsNullOrEmpty(interval.Name) || !char.IsLetter(interval.Name[0])
            ? Utc + interval.WallOffset.ToString("+HH:mm", CultureInfo.InvariantCulture)
            : interval.Name;
    }

    /// <summary>
    /// Describes when a cron expression occurs, followed by the abbreviation of its time zone when it is not the
    /// one of the site, such as "At 03:00 AM (JST)".
    /// </summary>
    /// <param name="expression">The cron expression.</param>
    /// <param name="culture">The culture of the description.</param>
    /// <param name="timeZone">The time zone the expression applies to, <see langword="null"/> for UTC.</param>
    /// <param name="isSiteTimeZone">Whether the time zone is the one of the site, which is not told.</param>
    /// <param name="utcNow">The current date, in UTC.</param>
    /// <returns>The description, or <see langword="null"/> when the expression is not valid or can't be described.</returns>
    public static string Describe(string expression, CultureInfo culture, ITimeZone timeZone, bool isSiteTimeZone, DateTime utcNow)
    {
        var description = Describe(expression, culture);

        return description is null || isSiteTimeZone
            ? description
            : $"{description} ({GetAbbreviation(timeZone, utcNow)})";
    }

    /// <summary>
    /// Gets the occurrences of a cron expression that follow a date.
    /// </summary>
    /// <param name="expression">The cron expression.</param>
    /// <param name="after">The date the occurrences follow, in the time zone the expression applies to.</param>
    /// <param name="count">The number of occurrences to return.</param>
    /// <returns>The occurrences, in the same time zone, none when the expression is not valid or never occurs.</returns>
    public static IReadOnlyList<DateTime> GetNextOccurrences(string expression, DateTime after, int count)
    {
        if (!IsValid(expression) || count <= 0)
        {
            return [];
        }

        return CrontabSchedule.Parse(expression).GetNextOccurrences(after, DateTime.MaxValue).Take(count).ToArray();
    }
}
