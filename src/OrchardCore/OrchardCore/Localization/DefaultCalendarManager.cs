using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public class DefaultCalendarManager : ICalendarManager
    {
        private readonly IEnumerable<ICalendarSelector> _calendarSelectors;
        private CalendarName? _calendarName;

        public DefaultCalendarManager(IEnumerable<ICalendarSelector> calendarSelectors)
        {
            _calendarSelectors = calendarSelectors;
        }

        public async Task<CalendarName> GetCurrentCalendar()
        {
            if (_calendarName.HasValue)
            {
                return _calendarName.Value;
            }

            var calendarResults = new List<CalendarSelectorResult>();

            foreach (var calendarSelector in _calendarSelectors)
            {
                var calendarResult = await calendarSelector.GetCalendarAsync();

                if (calendarResult != null)
                {
                    calendarResults.Add(calendarResult);
                }
            }

            if (calendarResults.Count == 0)
            {
                return CalendarName.Unknown;
            }
            else if (calendarResults.Count > 1)
            {
                calendarResults.Sort((x, y) => y.Priority.CompareTo(x.Priority));
            }

            _calendarName = await calendarResults.First().CalendarName();

            return _calendarName.Value;
        }
    }
}