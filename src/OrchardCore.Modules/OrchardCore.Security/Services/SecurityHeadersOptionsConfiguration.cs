using Microsoft.Extensions.Options;

namespace OrchardCore.Security.Services
{
    public class SecurityHeadersOptionsConfiguration : IConfigureOptions<SecurityHeadersOptions>
    {
        private readonly SecurityHeadersService _securityHeadersService;

        public SecurityHeadersOptionsConfiguration(SecurityHeadersService securityHeadersService)
        {
            _securityHeadersService = securityHeadersService;
        }

        public void Configure(SecurityHeadersOptions options)
        {
            var securityHeadersSettings = _securityHeadersService.GetSettingsAsync()
                .GetAwaiter().GetResult();

            options.ReferrerPolicy = securityHeadersSettings.ReferrerPolicy;
        }
    }
}
