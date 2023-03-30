using Microsoft.Extensions.Options;
using OrchardCore.Security.Settings;

namespace OrchardCore.Security.Services
{
    public class SecuritySettingsConfiguration : IConfigureOptions<SecuritySettings>
    {
        private readonly ISecurityService _securityService;

        public SecuritySettingsConfiguration(ISecurityService securityService)
            => _securityService = securityService;

        public void Configure(SecuritySettings options)
        {
            var securitySettings = _securityService.GetSettingsAsync()
                .GetAwaiter().GetResult();

            options.ContentSecurityPolicy = securitySettings.ContentSecurityPolicy;
            options.ContentTypeOptions = securitySettings.ContentTypeOptions;
            options.PermissionsPolicy = securitySettings.PermissionsPolicy;
            options.ReferrerPolicy = securitySettings.ReferrerPolicy;
        }
    }
}
