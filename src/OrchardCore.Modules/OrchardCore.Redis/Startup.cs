using System;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
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
        private readonly string _tenant;
        private readonly IShellConfiguration _configuration;
        private readonly ILogger<RedisCacheStartup> _logger;

        public Startup(ShellSettings shellSettings, IShellConfiguration configuration, ILogger<RedisCacheStartup> logger)
        {
            _tenant = shellSettings.Name;
            _configuration = configuration;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var configuration = _configuration.GetSection("OrchardCore.Redis").GetValue<string>("Configuration");

            if (String.IsNullOrWhiteSpace(configuration))
            {
                _logger.LogError("Tenant '{TenantName}' does not have a valid Redis configuration.", _tenant);
                return;
            }

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RedisOptions>, RedisOptionsSetup>());

            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton(sp => (IModularTenantEvents)sp.GetRequiredService<IRedisService>());
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
            if (services.LastOrDefault(d => d.ServiceType == typeof(IRedisService)) == null)
            {
                _logger.LogError("'Redis Cache' is not active on tenant '{TenantName}' as there is no Redis configuration.", _tenant);
                return;
            }

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>());

            services.AddStackExchangeRedisCache(o => { });
            services.AddTransient<ITagCache, RedisTagCache>();
        }
    }

    [Feature("OrchardCore.Redis.DataProtection")]
    public class RedisDataProtectionStartup : StartupBase
    {
        private readonly string _tenant;
        private readonly ILogger<RedisCacheStartup> _logger;

        public RedisDataProtectionStartup(ShellSettings shellSettings, ILogger<RedisCacheStartup> logger)
        {
            _tenant = shellSettings.Name;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (services.LastOrDefault(d => d.ServiceType == typeof(IRedisService)) == null)
            {
                _logger.LogError("'Redis DataProtection' is not active on tenant '{TenantName}' as there is no Redis configuration.", _tenant);
                return;
            }

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<KeyManagementOptions>, RedisKeyManagementOptionsSetup>());
        }
    }
}
