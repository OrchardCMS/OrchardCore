using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Distributed.Drivers;
using OrchardCore.Distributed.Settings;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;


namespace OrchardCore.Distributed
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
        }
    }

    [Feature("OrchardCore.Distributed.Redis.Settings")]
    public class RedisSettingsStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, RedisSiteSettingsDisplayDriver>();
        }
    }

    [Feature("OrchardCore.Distributed.Redis.Cache")]
    public class DistributedRedisCacheStartup : StartupBase
    {
        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddDistributedRedisCache(o => { });
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var siteService = serviceProvider.GetRequiredService<ISiteService>();
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();

            var siteSettings = siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
            var options = serviceProvider.GetService<IOptions<RedisCacheOptions>>().Value;

            if (siteSettings.Has<RedisSettings>())
            {
                // Right now, only a string representing the configuration is available.
                // In the next version there will be a full 'ConfigurationOptions' object.
                options.Configuration = siteSettings.As<RedisSettings>().Configuration;
            }

            options.InstanceName = shellSettings.Name;
        }
    }
}
