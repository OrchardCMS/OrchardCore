using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;

namespace OrchardCore.Localization
{
    public class DefaultCalendarManager : ICalendarManager
    {
        private readonly ICalendarSelector _calendarSelector;

        private static IDictionary<string, CalendarSystem> CalendarsList = CalendarSystem.Ids
            .ToLookup(c => CalendarSystem.ForId(c).Name)
            .ToDictionary(c => c.Key, c => CalendarSystem.ForId(c.First()));

        public DefaultCalendarManager(ICalendarSelector calendarSelector)
        {
            _calendarSelector = calendarSelector;
        }

        public IEnumerable<string> GetCalendars() => CalendarsList.Keys;

        public CalendarSystem GetCalendarByName(string calendarName)
        {
            if (String.IsNullOrEmpty(calendarName))
            {
                throw new ArgumentNullException(nameof(calendarName));
            }

            if (!CalendarsList.ContainsKey(calendarName))
            {
                throw new ArgumentException($"The calendar name '{calendarName}' is not a recognized System.Globalization calendar name.", nameof(calendarName));
            }

            return CalendarsList[calendarName];
        }

        public async Task<CalendarSystem> GetCurrentCalendar()
        {
            var calendarResult = await _calendarSelector.GetCalendar();
            var calendarName = await calendarResult.CalendarName();

            return CalendarsList[calendarName];
        }
    }
}