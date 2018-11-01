using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.ReverseProxy.Settings;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy.Services
{
    public class ReverseProxyService
    {
        private readonly ISiteService _siteService;

        public ReverseProxyService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<ReverseProxySettings> GetSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            return siteSettings.As<ReverseProxySettings>();
        }
    }
}
