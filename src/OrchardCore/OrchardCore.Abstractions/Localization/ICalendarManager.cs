using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a contract for manage calendars.
    /// </summary>
    public interface ICalendarManager
    {
        /// <summary>
        /// Gets the current calendar.
        /// </summary>
        /// <returns>The current calendar name.</returns>
        Task<CalendarName> GetCurrentCalendar();
    }
}
