using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.Security.Services
{
    public class SecurityHeadersService
    {
        private readonly ISiteService _siteService;

        public SecurityHeadersService(ISiteService siteService)
        {
            _siteService = siteService;
        }

        public async Task<SecurityHeadersOptions> GetSettingsAsync()
        {
            var securityHeadersSettings = await _siteService.GetSiteSettingsAsync();

            return securityHeadersSettings.As<SecurityHeadersOptions>();
        }
    }
}
