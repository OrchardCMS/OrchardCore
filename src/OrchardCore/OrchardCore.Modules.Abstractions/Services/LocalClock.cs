using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Modules
{
    public class LocalClock : ILocalClock
    {
        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;
        private readonly IClock _clock;
        private Task<ITimeZone> _timeZone;

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
            if (_timeZone == null)
            {
                _timeZone = LoadLocalTimeZoneAsync();
            }

            return _timeZone;
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

            timeZoneResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));

            return _clock.GetTimeZone(timeZoneResults[0].Id);
        }
    }
}
