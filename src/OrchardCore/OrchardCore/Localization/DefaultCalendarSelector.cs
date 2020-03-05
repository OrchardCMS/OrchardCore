using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a default implementation for <see cref="ICalendarSelector"/>.
    /// </summary>
    public class DefaultCalendarSelector : ICalendarSelector
    {
        private static readonly Task<CalendarSelectorResult> CalendarResult =
            Task.FromResult(new CalendarSelectorResult
            {
                Priority = 0,
                CalendarName = () =>
                {
                    return Task.FromResult(BclCalendars.GetCalendarName(CultureInfo.CurrentUICulture.Calendar));
                }
            });

        /// <inheritdocs />
        public Task<CalendarSelectorResult> GetCalendarAsync()
        {
            return CalendarResult;
        }
    }
}
