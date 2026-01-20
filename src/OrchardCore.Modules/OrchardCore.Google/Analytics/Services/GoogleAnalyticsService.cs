using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Google.Analytics.Services
{
    public class GoogleAnalyticsService : IGoogleAnalyticsService
    {
        private readonly ISiteService _siteService;

        public GoogleAnalyticsService(
            ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<GoogleAnalyticsSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GoogleAnalyticsSettings>();
        }
    }
}
