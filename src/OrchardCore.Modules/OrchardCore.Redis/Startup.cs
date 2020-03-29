using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Redis.Options;
using OrchardCore.Redis.Services;

namespace OrchardCore.Redis
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RedisOptions>, RedisOptionsSetup>());

            services.AddSingleton<IRedisService, RedisService>();
        }
    }

    [Feature("OrchardCore.Redis.Cache")]
    public class RedisCacheStartup : StartupBase
    {
        private readonly string _tenant;
        private readonly IShellConfiguration _configuration;
        private readonly ILogger<RedisCacheStartup> _logger;

        public RedisCacheStartup(ShellSettings shellSettings, IShellConfiguration configuration, ILogger<RedisCacheStartup> logger)
        {
            _tenant = shellSettings.Name;
            _configuration = configuration;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>());

            var configuration = _configuration.GetSection("OrchardCore.Redis").GetValue<string>("Configuration");

            if (String.IsNullOrWhiteSpace(configuration))
            {
                _logger.LogError("'Redis Cache' is not active on tenant '{TenantName}' as it does not have a valid Redis configuration.", _tenant);
                return;
            }

            services.AddStackExchangeRedisCache(o => { });
            services.AddTransient<ITagCache, RedisTagCache>();
        }
    }

    [Feature("OrchardCore.Redis.DataProtection")]
    public class RedisDataProtectionStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<KeyManagementOptions>, RedisKeyManagementOptionsSetup>());
        }
    }
}
