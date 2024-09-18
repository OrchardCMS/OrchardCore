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

        var tag = new TagBuilder("span");

        tag.AddCssClass("timespan-preview-value");

        tag.Attributes["title"] = timeSpan.ToString();

        tag.InnerHtml.AppendHtml(GetDuration(timeSpan.Value));

        return tag;
    }

    private LocalizedHtmlString GetDuration(TimeSpan timeSpan)
    {
        var days = timeSpan.Days;
        var hours = timeSpan.Hours;
        var minutes = timeSpan.Minutes;
        var seconds = timeSpan.Seconds;

        if (days > 0)
        {
            return GetDurationInDays(timeSpan, days, hours, minutes, seconds);
        }

        if (hours > 0)
        {
            return GetDurationInHours(timeSpan, hours, minutes, seconds);
        }

        if (minutes > 0)
        {
            return GetDurationInMinutes(timeSpan, minutes, seconds);
        }

        return H.Plural(seconds, "One second", "{0} seconds");
    }

    private LocalizedHtmlString GetDurationInMinutes(TimeSpan timeSpan, int minutes, int seconds)
    {
        if (seconds == timeSpan.TotalSeconds)
        {
            return H.Plural(minutes, "One minute", "{0} minutes");
        }

        return H.Plural(minutes, "Approximately a minute", "Approximately {0} minutes");
    }

    private LocalizedHtmlString GetDurationInHours(TimeSpan timeSpan, int hours, int minutes, int seconds)
    {
        if (hours == timeSpan.TotalHours)
        {
            return H.Plural(hours, "1 hour", "{0} hours");
        }

        if (minutes == 0)
        {
            return H.Plural(Math.Round(timeSpan.TotalHours), "Approximately an hour", "Approximately {0} hours");
        }

        if (hours == 1)
        {
            if (seconds > 0)
            {
                return H.Plural(minutes, "Approximately one hour and one minute", "Approximately one hour and {0} minutes");
            }

            return H.Plural(minutes, "One hour and one minute", "One hour and {0} minutes");
        }

        if (seconds > 0)
        {
            return H.Plural(minutes, "Approximately {1} hours and one minute", "Approximately {1} hours and {0} minutes", hours);
        }

        return H.Plural(minutes, "{1} hours and one minute", "{1} hours and {0} minutes", hours);

    }

    private LocalizedHtmlString GetDurationInDays(TimeSpan timeSpan, int days, int hours, int minutes, int seconds)
    {
        if (days == timeSpan.TotalDays)
        {
            return H.Plural(days, "One day", "{0} days");
        }

        if (hours == 0)
        {
            return H.Plural(days, "Approximately a day", "Approximately {0} days");
        }

        if (days == 1)
        {
            if (minutes + seconds > 0)
            {
                return H.Plural(hours, "Approximately one day and one hour", "Approximately one day and {0} hours");
            }

            return H.Plural(hours, "One day and one hour", "One day and {0} hours");
        }

        if (minutes + seconds > 0)
        {
            return H.Plural(hours, "Approximately {1} day and one hour", "Approximately {1} day and {0} hours", days);
        }

        return H.Plural(hours, "{1} day and one hour", "{1} day and {0} hours", days);
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
