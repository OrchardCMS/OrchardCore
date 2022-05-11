using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Security.Options;
using OrchardCore.Settings;

namespace OrchardCore.Security.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ISiteService _siteService;

        public SecurityService(ISiteService siteService)
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
