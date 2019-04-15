using System;
using System.Collections.Generic;
using System.Globalization;
using NodaTime;

namespace OrchardCore.Modules
{
    internal static class BclCalendars
    {
        public static IEnumerable<Calendar> MappedCalendars =>
            new[] { Hebrew, Hijri, Gregorian, Julian, Persian, UmAlQura };

        public static Calendar Hebrew => new HebrewCalendar();
        public static Calendar Hijri => new HijriCalendar();
        public static Calendar Gregorian => new GregorianCalendar();
        public static Calendar Julian => new JulianCalendar();
        public static Calendar Persian => new PersianCalendar();
        public static Calendar UmAlQura => new UmAlQuraCalendar();

        public static CalendarSystem ConvertToCalendarSystem(Calendar calendar)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException(nameof(calendar));
            }

            var calendarType = calendar.GetType();
            if (calendarType == typeof(GregorianCalendar))
            {
                return CalendarSystem.Iso;
            }
            else if (calendarType == typeof(HebrewCalendar))
            {
                return CalendarSystem.HebrewCivil;
            }
            else if (calendarType == typeof(HijriCalendar))
            {
                return CalendarSystem.IslamicBcl;
            }
            else if (calendarType == typeof(JulianCalendar))
            {
                return CalendarSystem.Julian;
            }
            else if (calendarType == typeof(PersianCalendar))
            {
                return calendar.IsLeapYear(1)
                    ? CalendarSystem.PersianSimple
                    : CalendarSystem.PersianAstronomical;
            }
            else if (calendarType == typeof(UmAlQuraCalendar))
            {
                return CalendarSystem.UmAlQura;
            }
            else
                return null;
        }
    }
}