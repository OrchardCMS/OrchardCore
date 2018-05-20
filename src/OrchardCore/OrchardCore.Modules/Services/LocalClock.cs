using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Modules;
using NodaTime;

namespace OrchardCore.Modules
{
    public class LocalClock : ILocalClock
    {
        private static readonly Task<ITimeZone> Empty = Task.FromResult<ITimeZone>(null);

        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;
        private readonly IClock _clock;
        private Task<ITimeZone> _timeZone = Empty;

        public LocalClock(IEnumerable<ITimeZoneSelector> timeZoneSelectors, IClock clock)
        {
            _timeZoneSelectors = timeZoneSelectors;
            _clock = clock;
        }

        public Task<DateTimeOffset> LocalNowAsync
        {
            get
            {
                return GetLocalTimeZoneAsync().ContinueWith(x => _clock.ConvertToTimeZone(_clock.UtcNow, x.Result));
            }
        }

        public Task<ITimeZone> GetLocalTimeZoneAsync()
        {
            // Caching the result per request
            if (_timeZone == Empty)
            {
                _timeZone = LoadLocalTimeZoneAsync();
            }

            return _timeZone;
        }

        public Task<DateTimeOffset> ConvertToLocalAsync(DateTimeOffset dateTimeOffSet)
        {
            return GetLocalTimeZoneAsync().ContinueWith(localTimeZone =>
            {
                var dateTimeZone = ((TimeZone)localTimeZone.Result).DateTimeZone;
                var offsetDateTime = OffsetDateTime.FromDateTimeOffset(dateTimeOffSet);
                return offsetDateTime.InZone(dateTimeZone).ToDateTimeOffset();
            });
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

            foreach(var result in timeZoneResults)
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
