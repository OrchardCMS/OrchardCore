using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Settings;

namespace OrchardCore.Security.Services
{
    public class SecuritySettingsConfiguration : IAsyncConfigureOptions<SecuritySettings>
    {
        private readonly ISecurityService _securityService;

        public SecuritySettingsConfiguration(ISecurityService securityService)
            => _securityService = securityService;

        public async ValueTask ConfigureAsync(SecuritySettings options)
        {
            var securitySettings = await _securityService.GetSettingsAsync();

            options.ContentSecurityPolicy = securitySettings.ContentSecurityPolicy;
            options.ContentTypeOptions = securitySettings.ContentTypeOptions;
            options.PermissionsPolicy = securitySettings.PermissionsPolicy;
            options.ReferrerPolicy = securitySettings.ReferrerPolicy;
        }
    }
}
