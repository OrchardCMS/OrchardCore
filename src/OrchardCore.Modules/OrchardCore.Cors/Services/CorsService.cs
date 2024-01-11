using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Cors.Settings;
using OrchardCore.Entities;
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

        internal async Task UpdateSettingsAsync(CorsSettings corsSettings)
        {
            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            siteSettings.Properties[nameof(CorsSettings)] = JObject.FromObject(corsSettings);
            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }
    }
}
