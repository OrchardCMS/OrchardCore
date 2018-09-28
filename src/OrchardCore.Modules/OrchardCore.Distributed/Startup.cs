using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

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

    [Feature("OrchardCore.Distributed.Redis.Cache")]
    public class DistributedRedisCacheStartup : StartupBase
    {
        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            //services.Configure<RedisCacheOptions>(o =>
            //{
            //    o.Configuration = "192.168.99.100:6379";
            //});

            services.AddDistributedRedisCache(o => { });
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var shellSettings = serviceProvider.GetRequiredService<ShellSettings>();
            var options = serviceProvider.GetService<IOptions<RedisCacheOptions>>().Value;

            options.Configuration = "192.168.99.100:6379,abortConnect=false";
            options.InstanceName = shellSettings.Name;
        }
    }
}
