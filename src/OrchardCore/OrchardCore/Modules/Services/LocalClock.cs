using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using OrchardCore.Localization;

namespace OrchardCore.Modules
{
    public class LocalClock : ILocalClock
    {
        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;
        private readonly IClock _clock;
        private readonly ICalendarManager _calendarManager;
        private ITimeZone _timeZone;

        public LocalClock(IEnumerable<ITimeZoneSelector> timeZoneSelectors, IClock clock, ICalendarManager calendarManager)
        {
            _timeZoneSelectors = timeZoneSelectors;
            _clock = clock;
            _calendarManager = calendarManager;
        }

        public Task<DateTimeOffset> LocalNowAsync
        {
            get
            {
                return GetLocalNowAsync();
            }
        }

        private async Task<DateTimeOffset> GetLocalNowAsync()
        {
            return _clock.ConvertToTimeZone(_clock.UtcNow, await GetLocalTimeZoneAsync());
        }

        // Caching the result per request.
        public async Task<ITimeZone> GetLocalTimeZoneAsync() => _timeZone ??= await LoadLocalTimeZoneAsync();

        public async Task<DateTimeOffset> ConvertToLocalAsync(DateTimeOffset dateTimeOffSet)
        {
            var localTimeZone = await GetLocalTimeZoneAsync();
            var dateTimeZone = ((TimeZone)localTimeZone).DateTimeZone;
            var offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet);
            var currentCalendar = BclCalendars.GetCalendarByName(await _calendarManager.GetCurrentCalendar());

            return offsetDateTime.InZone(dateTimeZone).WithCalendar(currentCalendar).ToDateTimeOffset();
        }

        public async Task<DateTime> ConvertToUtcAsync(DateTime dateTime)
        {
            var localTimeZone = await GetLocalTimeZoneAsync();
            var dateTimeZone = ((TimeZone)localTimeZone).DateTimeZone;
            var localDate = LocalDateTime.FromDateTime(dateTime);
            return dateTimeZone.AtStrictly(localDate).ToDateTimeUtc();
        }

        private async Task<ITimeZone> LoadLocalTimeZoneAsync()
        {
            var timeZoneResults = new List<TimeZoneSelectorResult>();

            foreach (var timeZoneSelector in _timeZoneSelectors)
            {
                var timeZoneResult = await timeZoneSelector.GetTimeZoneAsync();

                if (timeZoneResult != null)
                {
                    timeZoneResults.Add(timeZoneResult);
                }
            }

            if (timeZoneResults.Count == 0)
            {
                return _clock.GetSystemTimeZone();
            }
            else if (timeZoneResults.Count > 1)
            {
                timeZoneResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            }

            foreach (var result in timeZoneResults)
            {
                var value = await result.TimeZoneId();

                if (!String.IsNullOrEmpty(value))
                {
                    return _clock.GetTimeZone(value);
                }
            }

            return _clock.GetSystemTimeZone();
        }
    }
}
