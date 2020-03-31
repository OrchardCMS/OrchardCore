using System;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Distributed;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
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
        private readonly ILogger<Startup> _logger;

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
                var options = ConfigurationOptions.Parse(_configuration["OrchardCore.Redis:Configuration"]);
                services.Configure<RedisOptions>(o => o.ConfigurationOptions = options);
            }
            catch
            {
                _logger.LogError("'Redis' is not active on tenant '{TenantName}' as there is no valid Redis configuration.", _tenant);
                return;
            }

            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<IModularTenantEvents>(sp => sp.GetRequiredService<IRedisService>());
        }
    }

    [Feature("OrchardCore.Redis.Cache")]
    public class RedisCacheStartup : StartupBase
    {
        private readonly string _tenant;
        private readonly ILogger<RedisCacheStartup> _logger;

        public RedisCacheStartup(ShellSettings shellSettings, ILogger<RedisCacheStartup> logger)
        {
            _tenant = shellSettings.Name;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (!services.CheckRedisService("'Redis Cache'", _tenant, _logger))
            {
                return;
            }

            services.AddStackExchangeRedisCache(o => { });
            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>());
            services.AddTransient<ITagCache, RedisTagCache>();
        }
    }

    [Feature("OrchardCore.Redis.Bus")]
    public class RedisBusStartup : StartupBase
    {
        private readonly string _tenant;
        private readonly ILogger<RedisBusStartup> _logger;

        public RedisBusStartup(ShellSettings shellSettings, ILogger<RedisBusStartup> logger)
        {
            _tenant = shellSettings.Name;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (!services.CheckRedisService("'Redis Bus'", _tenant, _logger))
            {
                return;
            }

            services.AddSingleton<IMessageBus, RedisBus>();
        }
    }

    [Feature("OrchardCore.Redis.DataProtection")]
    public class RedisDataProtectionStartup : StartupBase
    {
        private readonly string _tenant;
        private readonly ILogger<RedisDataProtectionStartup> _logger;

        public RedisDataProtectionStartup(ShellSettings shellSettings, ILogger<RedisDataProtectionStartup> logger)
        {
            _tenant = shellSettings.Name;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (!services.CheckRedisService("'Redis DataProtection'", _tenant, _logger))
            {
                return;
            }

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<KeyManagementOptions>, RedisKeyManagementOptionsSetup>());
        }
    }

    internal static class ServiceCollectionExtensions
    {
        public static bool CheckRedisService(this IServiceCollection services, string feature, string tenant, ILogger logger)
        {
            if (services.FirstOrDefault(d => d.ServiceType == typeof(IRedisService)) == null)
            {
                logger.LogError(feature + " is not active on tenant '{TenantName}' as there is no valid Redis configuration.", tenant);
                return false;
            }

            return true;
        }
    }
}
