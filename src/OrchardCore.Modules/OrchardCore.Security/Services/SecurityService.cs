using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Security.Settings;
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

        public async Task<SecuritySettings> GetSettingsAsync()
        {
            var securityHeadersSettings = await _siteService.GetSiteSettingsAsync();

            return securityHeadersSettings.As<SecuritySettings>();
        }
    }
}
