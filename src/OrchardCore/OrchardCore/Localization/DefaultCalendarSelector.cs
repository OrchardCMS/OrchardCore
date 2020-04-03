using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
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

        public Task<CalendarSelectorResult> GetCalendarAsync()
        {
            return CalendarResult;
        }
    }
}
