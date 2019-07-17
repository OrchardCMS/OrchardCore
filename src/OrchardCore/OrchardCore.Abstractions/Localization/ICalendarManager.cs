using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public interface ICalendarManager
    {
        Task<CalendarName> GetCurrentCalendar();
    }
}