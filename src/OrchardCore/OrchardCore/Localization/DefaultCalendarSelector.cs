using System;
using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public class DefaultCalendarSelector : ICalendarSelector
    {
        private static readonly Func<Task<CalendarName>> GetCalendarName = () => {
                    var calendar = BclCalendars.ConvertToCalendarSystem(CultureInfo.CurrentUICulture.Calendar);
                    var calendarName = BclCalendars.GetCalendarName(calendar);

                    return Task.FromResult(calendarName);
         };

        public Task<CalendarSelectorResult> GetCalendarAsync()
        {
            return Task.FromResult(new CalendarSelectorResult
            {
                Priority = 0,
                CalendarName = GetCalendarName
            });
        }
    }
}
