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
            IPluralStringLocalizer<DateTimeShapes> localizer,
            ILocalClock localClock
            )
        {
            _localClock = localClock;
            _clock = clock;
            T = localizer;
        }

        IPluralStringLocalizer T { get; }

        [Shape]
        public IHtmlContent TimeSpan(IHtmlHelper Html, DateTime? Utc, DateTime? Origin)
        {
            Utc = Utc ?? _clock.UtcNow;
            Origin = Origin ?? _clock.UtcNow;

            var time = _clock.UtcNow - Utc.Value;

            if (time.TotalYears() > 1)
                return Html.Raw(Html.Encode(T["1 year ago", "{0} years ago", time.TotalYears(), time.TotalYears()].Value));
            if (time.TotalYears() < -1)
                return Html.Raw(Html.Encode(T["in 1 year", "in {0} years", -time.TotalYears(), -time.TotalYears()].Value));

            if (time.TotalMonths() > 1)
                return Html.Raw(Html.Encode(T["1 month ago", "{0} months ago", time.TotalMonths(), time.TotalMonths()].Value));
            if (time.TotalMonths() < -1)
                return Html.Raw(Html.Encode(T["in 1 month", "in {0} months", -time.TotalMonths(), -time.TotalMonths()].Value));

            if (time.TotalWeeks() > 1)
                return Html.Raw(Html.Encode(T["1 week ago", "{0} weeks ago", time.TotalWeeks(), time.TotalWeeks()].Value));
            if (time.TotalWeeks() < -1)
                return Html.Raw(Html.Encode(T["in 1 week", "in {0} weeks", -time.TotalWeeks(), -time.TotalWeeks()].Value));

            if (time.TotalHours > 24)
                return Html.Raw(Html.Encode(T["1 day ago", "{0} days ago", time.Days, time.Days].Value));
            if (time.TotalHours < -24)
                return Html.Raw(Html.Encode(T["in 1 day", "in {0} days", -time.Days, -time.Days].Value));

            if (time.TotalMinutes > 60)
                return Html.Raw(Html.Encode(T["1 hour ago", "{0} hours ago", time.Hours, time.Hours].Value));
            if (time.TotalMinutes < -60)
                return Html.Raw(Html.Encode(T["in 1 hour", "in {0} hours", -time.Hours, -time.Hours].Value));

            if (time.TotalSeconds > 60)
                return Html.Raw(Html.Encode(T["1 minute ago", "{0} minutes ago", time.Minutes, time.Minutes].Value));
            if (time.TotalSeconds < -60)
                return Html.Raw(Html.Encode(T["in 1 minute", "in {0} minutes", -time.Minutes, -time.Minutes].Value));

            if (time.TotalSeconds > 10)
                return Html.Raw(Html.Encode(T["1 second ago", "{0} seconds ago", time.Seconds, time.Seconds].Value)); //aware that the singular won't be used
            if (time.TotalSeconds < -10)
                return Html.Raw(Html.Encode(T["in 1 second", "in {0} seconds", -time.Seconds, -time.Seconds].Value));

            return time.TotalMilliseconds > 0
                       ? Html.Raw(Html.Encode(T["a moment ago", "a moment ago", 0]))
                       : Html.Raw(Html.Encode(T["in a moment", "in a moment", 0]));
        }

        [Shape]
        public async Task<IHtmlContent> DateTime(IHtmlHelper Html, DateTime? Utc, string Format)
        {
            Utc = Utc ?? _clock.UtcNow;
            var zonedTime = await _localClock.ConvertToLocalAsync(Utc.Value);

            if (Format == null)
            {
                // We use the Plural localizer so we don't need to resolve another one.
                // Internally it will use the string localizer by passing 0 as count.
                Format = T[LongDateTimeFormat, LongDateTimeFormat, 0].Value;
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