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

            options.ContentTypeOptions = securitySettings.ContentTypeOptions;
            options.FrameOptions = securitySettings.FrameOptions;
            options.PermissionsPolicy = securitySettings.PermissionsPolicy;
            options.PermissionsPolicyOrigin = securitySettings.PermissionsPolicyOrigin;
            options.ReferrerPolicy = securitySettings.ReferrerPolicy;
        }
    }
}
