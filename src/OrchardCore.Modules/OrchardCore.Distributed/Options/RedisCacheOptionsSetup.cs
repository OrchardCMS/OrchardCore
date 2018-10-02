using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed.Settings;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;

namespace OrchardCore.Distributed.Options
{
    public class RedisCacheOptionsSetup : IConfigureOptions<RedisCacheOptions>
    {
        private readonly ShellSettings _shellSettings;
        private readonly ISiteService _siteService;

        public RedisCacheOptionsSetup(ShellSettings shellSettings, ISiteService siteService)
        {
            _shellSettings = shellSettings;
            _siteService = siteService;
        }

        public void Configure(RedisCacheOptions options)
        {
            var siteSettings = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

            if (siteSettings.Has<RedisSettings>())
            {
                // Right now, only a string representing the configuration is available.
                // In the next version there will be a full 'ConfigurationOptions' object.
                options.Configuration = siteSettings.As<RedisSettings>().Configuration;
            }

            options.InstanceName = _shellSettings.Name;
        }
    }
}
