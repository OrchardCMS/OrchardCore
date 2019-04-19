using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Resources
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private readonly ISiteService _siteService;
        private readonly IHostEnvironment _env;

        public ResourceManagementOptionsConfiguration(ISiteService siteService, IHostEnvironment env)
        {
            _siteService = siteService;
            _env = env;
        }

        public void Configure(ResourceManagementOptions options)
        {
            var settings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            switch(settings.ResourceDebugMode)
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
        }
    }
}
