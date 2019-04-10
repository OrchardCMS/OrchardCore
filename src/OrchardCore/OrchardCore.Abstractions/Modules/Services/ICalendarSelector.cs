using System;
using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public interface ICalendarSelector
    {
        Task<CalendarSelectorResult> GetCalendar();
    }
}
