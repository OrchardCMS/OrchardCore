using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Cors.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Cors.Services
{
    public class CorsService
    {
        private readonly ISiteService _siteService;

        public CorsService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<CorsSettings> GetSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.As<CorsSettings>();
        }
    }
}
