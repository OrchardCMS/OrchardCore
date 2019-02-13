using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.Shapes
{
    public class DateTimeShapes : IShapeAttributeProvider
    {
        private const string LongDateTimeFormat = "dddd, MMMM d, yyyy h:mm:ss tt";
        private readonly IClock _clock;
        private readonly ILocalClock _localClock;

        public DateTimeShapes(
            IClock clock,
            IStringLocalizer<DateTimeShapes> localizer,
            ILocalClock localClock
            )
        {
            _localClock = localClock;
            _clock = clock;
            T = localizer;
        }

        IStringLocalizer T { get; }

        [Shape]
        public IHtmlContent TimeSpan(IHtmlHelper Html, DateTime? Utc, DateTime? Origin)
        {
            Utc = Utc ?? _clock.UtcNow;
            Origin = Origin ?? _clock.UtcNow;

            var time = _clock.UtcNow - Utc.Value;

            if (time.TotalYears() > 1)
                return Html.Raw(Html.Encode(T.Plural(time.TotalYears(), "1 year ago", "{0} years ago").Value));
            if (time.TotalYears() < -1)
                return Html.Raw(Html.Encode(T.Plural(-time.TotalYears(), "in 1 year", "in {0} years").Value));

            if (time.TotalMonths() > 1)
                return Html.Raw(Html.Encode(T.Plural(time.TotalMonths(), "1 month ago", "{0} months ago").Value));
            if (time.TotalMonths() < -1)
                return Html.Raw(Html.Encode(T.Plural(-time.TotalMonths(), "in 1 month", "in {0} months").Value));

            if (time.TotalWeeks() > 1)
                return Html.Raw(Html.Encode(T.Plural(time.TotalWeeks(), "1 week ago", "{0} weeks ago").Value));
            if (time.TotalWeeks() < -1)
                return Html.Raw(Html.Encode(T.Plural(-time.TotalWeeks(), "in 1 week", "in {0} weeks").Value));

            if (time.TotalHours > 24)
                return Html.Raw(Html.Encode(T.Plural(time.Days, "1 day ago", "{0} days ago").Value));
            if (time.TotalHours < -24)
                return Html.Raw(Html.Encode(T.Plural(-time.Days, "in 1 day", "in {0} days").Value));

            if (time.TotalMinutes > 60)
                return Html.Raw(Html.Encode(T.Plural(time.Hours, "1 hour ago", "{0} hours ago").Value));
            if (time.TotalMinutes < -60)
                return Html.Raw(Html.Encode(T.Plural(-time.Hours, "in 1 hour", "in {0} hours").Value));

            if (time.TotalSeconds > 60)
                return Html.Raw(Html.Encode(T.Plural(time.Minutes, "1 minute ago", "{0} minutes ago").Value));
            if (time.TotalSeconds < -60)
                return Html.Raw(Html.Encode(T.Plural(-time.Minutes, "in 1 minute", "in {0} minutes").Value));

            if (time.TotalSeconds > 10)
                return Html.Raw(Html.Encode(T.Plural(time.Seconds, "1 second ago", "{0} seconds ago").Value)); //aware that the singular won't be used
            if (time.TotalSeconds < -10)
                return Html.Raw(Html.Encode(T.Plural(-time.Seconds, "in 1 second", "in {0} seconds").Value));

            return time.TotalMilliseconds > 0
                       ? Html.Raw(Html.Encode(T["a moment ago"]))
                       : Html.Raw(Html.Encode(T["in a moment"]));
        }

        [Shape]
        public async Task<IHtmlContent> DateTime(IHtmlHelper Html, DateTime? Utc, string Format)
        {
            Utc = Utc ?? _clock.UtcNow;
            var zonedTime = await _localClock.ConvertToLocalAsync(Utc.Value);

            if (Format == null)
            {
                Format = T[LongDateTimeFormat].Value;
            }

            return Html.Raw(Html.Encode(zonedTime.ToString(Format, CultureInfo.CurrentUICulture)));
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
}