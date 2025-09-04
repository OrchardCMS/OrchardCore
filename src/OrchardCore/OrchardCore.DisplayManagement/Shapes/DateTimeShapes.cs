using System.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Shapes;

[Feature(Application.DefaultFeatureId)]
public class DateTimeShapes : IShapeAttributeProvider
{
    private const string LongDateTimeFormat = "dddd, MMMM d, yyyy h:mm:ss tt";
    private readonly IClock _clock;
    private readonly ILocalClock _localClock;
    protected readonly IStringLocalizer S;
    protected readonly IHtmlLocalizer H;

    public DateTimeShapes(
        IClock clock,
        IStringLocalizer<DateTimeShapes> localizer,
        IHtmlLocalizer<DateTimeShapes> htmlLocalizer,
        ILocalClock localClock
        )
    {
        _localClock = localClock;
        _clock = clock;
        S = localizer;
        H = htmlLocalizer;
    }

    [Shape]
    public IHtmlContent TimeSpan(DateTime? Utc, DateTime? Origin)
    {
        Utc ??= _clock.UtcNow;
        Origin ??= _clock.UtcNow;

        var time = Origin.Value - Utc.Value;

        if (time.TotalYears() > 1)
        {
            return H.Plural(time.TotalYears(), "1 year ago", "{0} years ago");
        }

        if (time.TotalYears() < -1)
        {
            return H.Plural(-time.TotalYears(), "in 1 year", "in {0} years");
        }

        if (time.TotalMonths() > 1)
        {
            return H.Plural(time.TotalMonths(), "1 month ago", "{0} months ago");
        }

        if (time.TotalMonths() < -1)
        {
            return H.Plural(-time.TotalMonths(), "in 1 month", "in {0} months");
        }

        if (time.TotalWeeks() > 1)
        {
            return H.Plural(time.TotalWeeks(), "1 week ago", "{0} weeks ago");
        }

        if (time.TotalWeeks() < -1)
        {
            return H.Plural(-time.TotalWeeks(), "in 1 week", "in {0} weeks");
        }

        if (time.TotalHours > 24)
        {
            return H.Plural(time.Days, "1 day ago", "{0} days ago");
        }

        if (time.TotalHours < -24)
        {
            return H.Plural(-time.Days, "in 1 day", "in {0} days");
        }

        if (time.TotalMinutes > 60)
        {
            return H.Plural(time.Hours, "1 hour ago", "{0} hours ago");
        }

        if (time.TotalMinutes < -60)
        {
            return H.Plural(-time.Hours, "in 1 hour", "in {0} hours");
        }

        if (time.TotalSeconds > 60)
        {
            return H.Plural(time.Minutes, "1 minute ago", "{0} minutes ago");
        }

        if (time.TotalSeconds < -60)
        {
            return H.Plural(-time.Minutes, "in 1 minute", "in {0} minutes");
        }

        if (time.TotalSeconds > 10)
        {
            return H.Plural(time.Seconds, "1 second ago", "{0} seconds ago"); // Aware that the singular won't be used.
        }

        if (time.TotalSeconds < -10)
        {
            return H.Plural(-time.Seconds, "in 1 second", "in {0} seconds");
        }

        return time.TotalMilliseconds > 0
            ? H["a moment ago"]
            : H["in a moment"];
    }

    [Shape]
    public async Task<IHtmlContent> DateTime(IHtmlHelper Html, DateTime? Utc, string Format)
    {
        Utc ??= _clock.UtcNow;
        var zonedTime = await _localClock.ConvertToLocalAsync(Utc.Value);
        Format ??= S[LongDateTimeFormat].Value;

        return Html.Raw(Html.Encode(zonedTime.ToString(Format, CultureInfo.CurrentUICulture)));
    }

    [Shape]
    public IHtmlContent Duration(TimeSpan? timeSpan)
    {
        if (timeSpan == null)
        {
            return HtmlString.Empty;
        }

        if (timeSpan.Value.Days > 0)
        {
            return GetDurationInDays(timeSpan.Value);
        }

        if (timeSpan.Value.Hours > 0)
        {
            return GetDurationInHours(timeSpan.Value);
        }

        if (timeSpan.Value.Minutes > 0)
        {
            return GetDurationInMinutes(timeSpan.Value);
        }

        var seconds = timeSpan.Value.Seconds;

        if (seconds > 0)
        {
            return H.Plural(seconds, "1 second", "{0} seconds");
        }

        return H["less than a second"];
    }

    private LocalizedHtmlString GetDurationInDays(TimeSpan timeSpan)
    {
        var totalDays = timeSpan.TotalDays;

        if (timeSpan.Days == totalDays)
        {
            return H.Plural(timeSpan.Days, "1 day", "{0} days");
        }

        return H.Plural((int)Math.Round(totalDays), "approximately a day", "approximately {0} days");
    }

    private LocalizedHtmlString GetDurationInHours(TimeSpan timeSpan)
    {
        var totalHours = timeSpan.TotalHours;

        if (timeSpan.Hours == totalHours)
        {
            return H.Plural(timeSpan.Hours, "1 hour", "{0} hours");
        }

        return H.Plural((int)Math.Round(totalHours), "approximately an hour", "approximately {0} hours");
    }

    private LocalizedHtmlString GetDurationInMinutes(TimeSpan timeSpan)
    {
        var totalMinutes = timeSpan.TotalMinutes;

        if (timeSpan.Minutes == totalMinutes)
        {
            return H.Plural(timeSpan.Minutes, "1 minute", "{0} minutes");
        }

        return H.Plural((int)Math.Round(totalMinutes), "approximately a minute", "approximately {0} minutes");
    }
}

public static class TimespanExtensions
{
    public static int TotalWeeks(this TimeSpan time)
    {
        return (int)time.TotalDays / 7;
    }

    public static int TotalMonths(this TimeSpan time)
    {
        return (int)time.TotalDays / 31;
    }

    public static int TotalYears(this TimeSpan time)
    {
        return (int)time.TotalDays / 365;
    }
}
