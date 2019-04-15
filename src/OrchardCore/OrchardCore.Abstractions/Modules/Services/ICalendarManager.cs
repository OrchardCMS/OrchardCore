using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace OrchardCore.Modules
{
    public interface ICalendarManager
    {
        IEnumerable<string> GetCalendars();
        Task<CalendarSystem> GetCurrentCalendar();
        CalendarSystem GetCalendarByName(string calendarName);
    }
}