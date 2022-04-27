using Microsoft.Extensions.Options;

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

            options.ContentSecurityPolicy = securitySettings.ContentSecurityPolicy ?? SecurityHeaderDefaults.ContentSecurityPolicy;
            options.ContentTypeOptions = securitySettings.ContentTypeOptions ?? SecurityHeaderDefaults.ContentTypeOptions;
            options.FrameOptions = securitySettings.FrameOptions ?? SecurityHeaderDefaults.FrameOptions;
            options.PermissionsPolicy = securitySettings.PermissionsPolicy ?? SecurityHeaderDefaults.PermissionsPolicy;
            options.ReferrerPolicy = securitySettings.ReferrerPolicy ?? SecurityHeaderDefaults.ReferrerPolicy;
        }
    }
}
