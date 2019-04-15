using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Modules
{
    public class DefaultCalendarSelector : ICalendarSelector
    {
        private readonly IOrchardHelper _orchardHelper;

        public DefaultCalendarSelector(IOrchardHelper orchardHelper)
        {
            _orchardHelper = orchardHelper;
        }

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
