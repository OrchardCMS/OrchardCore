using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public interface ICalendarSelector
    {
        Task<CalendarSelectorResult> GetCalendarAsync();
    }
}
