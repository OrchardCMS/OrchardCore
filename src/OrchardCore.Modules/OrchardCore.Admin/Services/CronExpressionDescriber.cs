using System.Globalization;
using CronExpressionDescriptor;
using NCrontab;
using OrchardCore.Modules;

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
