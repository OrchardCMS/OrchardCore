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

            options.ReferrerPolicy = securitySettings.ReferrerPolicy;
        }
    }
}
