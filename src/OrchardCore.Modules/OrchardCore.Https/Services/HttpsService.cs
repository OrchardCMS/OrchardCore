using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Https.Settings;
using OrchardCore.Settings;

namespace OrchardCore.Https.Services
{
    public class HttpsService : IHttpsService
    {
        private readonly ISiteService _siteService;

        public HttpsService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<HttpsSettings> GetSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.As<HttpsSettings>();
        }
    }
}
