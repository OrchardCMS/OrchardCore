using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Cors.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Cors.Services
{
    public class CorsService
    {
        private readonly ISiteService _siteService;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public CorsService(
            ISiteService siteService,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _siteService = siteService;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public async Task<CorsSettings> GetSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.As<CorsSettings>();
        }

        internal async Task UpdateSettingsAsync(CorsSettings corsSettings)
        {
            var siteSettings = await _siteService.LoadSiteSettingsAsync();
            siteSettings.Properties[nameof(CorsSettings)] = JObject.FromObject(corsSettings, _jsonSerializerOptions);
            await _siteService.UpdateSiteSettingsAsync(siteSettings);
        }
    }
}
