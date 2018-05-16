using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.DisplayManagement.TimeZone
{
    public class TimeZoneManager : ITimeZoneManager
    {
        private readonly IEnumerable<ITimeZoneSelector> _timeZoneSelectors;
        private readonly IClock _clock;
        private ITimeZone _timeZone;

        public TimeZoneManager(IEnumerable<ITimeZoneSelector> timeZoneSelectors, IClock clock)
        {
            _timeZoneSelectors = timeZoneSelectors;
            _clock = clock;
        }

        public async Task<ITimeZone> GetTimeZoneAsync()
        {
            // For performance reason, processes the current timezone only once per scope (request).
            // This can't be cached as each request gets a different value.
            if (_timeZone == null)
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

                timeZoneResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));

                if (timeZoneResults.Count == 0)
                {
                    return null;
                }

                // load the TimeZone
                foreach (var timeZone in timeZoneResults)
                {
                    return _timeZone = _clock.GetLocalTimeZone(timeZone.Id);
                }

                // No valid TimeZone. Don't save the result right now.
                return null;
            }

            return _timeZone;
        }
    }
}
