using System;
using System.Globalization;
using NodaTime;

namespace OrchardCore.Localization
{
    internal static class BclCalendars
    {
        public readonly static Calendar Hebrew = new HebrewCalendar();
        public readonly static Calendar Hijri = new HijriCalendar();
        public readonly static Calendar Gregorian = new GregorianCalendar();
        public readonly static Calendar Julian = new JulianCalendar();
        public readonly static Calendar Persian = new PersianCalendar();
        public readonly static Calendar UmAlQura = new UmAlQuraCalendar();

        public static CalendarSystem GetCalendarByName(CalendarName calendarName) =>
            calendarName switch
            {
                CalendarName.Hebrew => CalendarSystem.HebrewCivil,
                CalendarName.Hijri => CalendarSystem.IslamicBcl,
                CalendarName.Gregorian => CalendarSystem.Iso,
                CalendarName.Julian => CalendarSystem.Julian,
                CalendarName.Persian => CultureInfo.CurrentUICulture.Calendar.IsLeapYear(1)
                    ? CalendarSystem.PersianSimple
                    : CalendarSystem.PersianAstronomical,
                CalendarName.UmAlQura => CalendarSystem.UmAlQura,
                _ => throw new NotSupportedException($"The calendar is not supported."),
            };

        public static CalendarName GetCalendarName(Calendar calendar)
        {
            if (calendar == null)
            {
                throw new ArgumentNullException(nameof(calendar));
            }

            var calendarType = calendar.GetType();
            if (calendarType == typeof(GregorianCalendar))
            {
                return CalendarName.Gregorian;
            }
            else if (calendarType == typeof(HebrewCalendar))
            {
                return CalendarName.Hebrew;
            }
            else if (calendarType == typeof(HijriCalendar))
            {
                return CalendarName.Hijri;
            }
            else if (calendarType == typeof(JulianCalendar))
            {
                return CalendarName.Julian;
            }
            else if (calendarType == typeof(PersianCalendar))
            {
                return CalendarName.Persian;
            }
            else if (calendarType == typeof(UmAlQuraCalendar))
            {
                return CalendarName.UmAlQura;
            }
            else
            {
                return CalendarName.Unknown;
            }
        }

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
            {
                return null;
            }
        }
    }
}
