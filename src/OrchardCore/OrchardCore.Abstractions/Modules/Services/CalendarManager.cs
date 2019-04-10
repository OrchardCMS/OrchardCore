using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace OrchardCore.Modules
{
    public class CalendarManager : ICalendarManager
    {
        private static IDictionary<string, Calendar> CalendarsList = new Dictionary<string, Calendar>
        {
            { "ChineseLunisolarCalendar", new ChineseLunisolarCalendar() },
			{ "GregorianCalendar", new GregorianCalendar() },
			{ "HebrewCalendar", new HebrewCalendar() },
			{ "HijriCalendar", new HijriCalendar() },
			{ "JapaneseCalendar", new JapaneseCalendar() },
			{ "JapaneseLunisolarCalendar", new JapaneseLunisolarCalendar() },
			{ "JulianCalendar", new JulianCalendar() },
			{ "KoreanCalendar", new KoreanCalendar() },
			{ "KoreanLunisolarCalendar", new KoreanLunisolarCalendar() },
			{ "PersianCalendar", new PersianCalendar() },
			{ "TaiwanCalendar", new TaiwanCalendar() },
			{ "TaiwanLunisolarCalendar", new TaiwanLunisolarCalendar() },
			{ "ThaiBuddhistCalendar", new ThaiBuddhistCalendar() },
            { "UmAlQuraCalendar", new UmAlQuraCalendar() }
        };
        // private readonly IEnumerable<ICalendarSelector> _calendarSelectors;

        // public CalendarManager(IEnumerable<ICalendarSelector> calendarSelectors)
        // {
		// 	_calendarSelectors = calendarSelectors;
        // }

        public IEnumerable<string> GetCalendars() => CalendarsList.Keys;

        public Calendar GetCalendarByName(string calendarName)
        {
            if (string.IsNullOrEmpty(calendarName))
            {
                throw new ArgumentNullException(nameof(calendarName));
            }
            
            if (!CalendarsList.ContainsKey(calendarName))
            {
                throw new ArgumentException($"The calendar name '{calendarName}' is not a recognized System.Globalization calendar name.", nameof(calendarName));
            }

            return CalendarsList[calendarName];
        }
    }
}