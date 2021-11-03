using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private readonly ISiteService _siteService;
        private readonly IHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _tenantPrefix;

        public ResourceManagementOptionsConfiguration(ISiteService siteService, IHostEnvironment env, IHttpContextAccessor httpContextAccessor, ShellSettings shellSettings)
        {
            _siteService = siteService;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _tenantPrefix = string.IsNullOrEmpty(shellSettings.RequestUrlPrefix) ? string.Empty : "/" + shellSettings.RequestUrlPrefix;
        }

        ResourceManifest BuildManifest() => ResourceManfiestGenerator.Build(_tenantPrefix);

        public void Configure(ResourceManagementOptions options)
        {
            options.ResourceManifests.Add(BuildManifest());

            var settings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            switch (settings.ResourceDebugMode)
            {
                case ResourceDebugMode.Enabled:
                    options.DebugMode = true;
                    break;

                case ResourceDebugMode.Disabled:
                    options.DebugMode = false;
                    break;

                case ResourceDebugMode.FromConfiguration:
                    options.DebugMode = !_env.IsProduction();
                    break;
            }

            options.UseCdn = settings.UseCdn;

            options.CdnBaseUrl = settings.CdnBaseUrl;

            options.AppendVersion = settings.AppendVersion;

            options.ContentBasePath = _httpContextAccessor.HttpContext.Request.PathBase.Value;
        }
    }
}
