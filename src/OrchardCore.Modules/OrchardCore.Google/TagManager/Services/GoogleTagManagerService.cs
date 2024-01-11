using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager.Services
{
    public class GoogleTagManagerService : IGoogleTagManagerService
    {
        private readonly ISiteService _siteService;

        public GoogleTagManagerService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<GoogleTagManagerSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<GoogleTagManagerSettings>();
        }
    }
}
