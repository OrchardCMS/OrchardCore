using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using OrchardCore.Settings;

namespace OrchardCore.DynamicCache
{
    public class CacheOptionsConfiguration : IConfigureOptions<CacheOptions>
    {
        private readonly ISiteService _siteService;
        private readonly IHostEnvironment _env;

        public CacheOptionsConfiguration(ISiteService siteService, IHostEnvironment env)
        {
            _siteService = siteService;
            _env = env;
        }

        public void Configure(CacheOptions options)
        {
            var settings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            switch (settings.CacheMode)
            {
                case CacheMode.Enabled:
                    options.Enabled = true;
                    break;

                case CacheMode.DebugEnabled:
                    options.Enabled = true;
                    options.DebugMode = true;
                    break;

                case CacheMode.Disabled:
                    options.Enabled = false;
                    break;

                case CacheMode.FromConfiguration:
                    options.Enabled = _env.IsProduction();
                    break;
            }
        }
    }
}
