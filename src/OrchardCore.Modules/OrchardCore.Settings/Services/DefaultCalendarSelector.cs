using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Settings.Services
{
    public class DefaultCalednarSelector : ICalendarSelector
    {
        private readonly ISiteService _siteService;

        public DefaultCalednarSelector(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public Task<CalendarSelectorResult> GetCalendar()
        {
            return Task.FromResult(new CalendarSelectorResult
            {
                Priority = 0,
                CalendarName = async () => (await _siteService.GetSiteSettingsAsync())?.Calendar
            });
        }
    }
}
