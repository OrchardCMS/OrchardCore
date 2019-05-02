using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public class DefaultCalendarSelector : ICalendarSelector
    {
        public Task<CalendarSelectorResult> GetCalendar()
        {
            return Task.FromResult(new CalendarSelectorResult
            {
                Priority = 0,
                CalendarName = () => {
                    var calendar = BclCalendars.ConvertToCalendarSystem(CultureInfo.CurrentUICulture.Calendar);
                    return Task.FromResult(calendar.Name);
                }
            });
        }
    }
}
