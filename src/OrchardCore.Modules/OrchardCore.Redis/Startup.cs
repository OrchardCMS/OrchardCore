using System;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Caching.Distributed;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;
using OrchardCore.Redis.Options;
using OrchardCore.Redis.Services;
using StackExchange.Redis;

namespace OrchardCore.Redis
{
    public class Startup : StartupBase
    {
        private readonly string _tenant;
        private readonly IShellConfiguration _configuration;
        private readonly ILogger _logger;

        public Startup(ShellSettings shellSettings, IShellConfiguration configuration, ILogger<Startup> logger)
        {
            _tenant = shellSettings.Name;
            _configuration = configuration;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            try
            {
                var configuration = _configuration["OrchardCore_Redis:Configuration"];
                var configurationOptions = ConfigurationOptions.Parse(configuration);
                var instancePrefix = _configuration["OrchardCore_Redis:InstancePrefix"];

                services.Configure<RedisOptions>(options =>
                {
                    options.Configuration = configuration;
                    options.ConfigurationOptions = configurationOptions;
                    options.InstancePrefix = instancePrefix;
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, "'Redis' features are not active on tenant '{TenantName}' as the 'Configuration' string is missing or invalid.", _tenant);
                return;
            }

            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<IModularTenantEvents>(sp => sp.GetRequiredService<IRedisService>());
            services.AddSingleton<IRedisDatabaseFactory, RedisDatabaseFactory>();
        }
    }

    [Feature("OrchardCore.Redis.Cache")]
    public class RedisCacheStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            if (services.Any(d => d.ServiceType == typeof(IRedisService)))
            {
                services.AddSingleton<IDistributedCache, RedisCacheWrapper>();
                services.AddTransient<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>();
                services.AddScoped<ITagCache, RedisTagCache>();
            }
        }
    }

    [Feature("OrchardCore.Redis.Bus")]
    public class RedisBusStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            if (services.Any(d => d.ServiceType == typeof(IRedisService)))
            {
                services.AddSingleton<IMessageBus, RedisBus>();
            }
        }
    }

    [Feature("OrchardCore.Redis.Lock")]
    public class RedisLockStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            if (services.Any(d => d.ServiceType == typeof(IRedisService)))
            {
                services.AddSingleton<IDistributedLock, RedisLock>();
            }
        }
    }

    [Feature("OrchardCore.Redis.DataProtection")]
    public class RedisDataProtectionStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            if (services.Any(d => d.ServiceType == typeof(IRedisService)))
            {
                services.AddTransient<IConfigureOptions<KeyManagementOptions>, RedisKeyManagementOptionsSetup>();
            }
        }
    }
}
