using System.Threading.Tasks;
using NodaTime;

namespace OrchardCore.Localization
{
    public interface ICalendarManager
    {
        Task<CalendarSystem> GetCurrentCalendar();
    }
}