using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

namespace OrchardCore.Redis;

public sealed class Startup : StartupBase
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
        var redisOptions = new RedisOptions().BindConfiguration(_configuration, _logger);
        var validationErrors = redisOptions.Validate();

        if (validationErrors.Any())
        {
            _logger.LogError("'Redis' features are not active on tenant '{TenantName}': {Errors}", _tenant, string.Join(' ', validationErrors.Select(error => error.ErrorMessage)));

            services.TryAddSingleton<IRedisService, NullRedisService>();
        }
        else
        {
            services.Configure<RedisOptions>(options =>
            {
                options.Configuration = redisOptions.Configuration;
                options.ConfigurationOptions = redisOptions.ConfigurationOptions;
                options.InstancePrefix = redisOptions.InstancePrefix;
            });

            services.AddSingleton<IRedisService, RedisService>();
            services.AddSingleton<IModularTenantEvents>(sp => sp.GetRequiredService<IRedisService>());
        }

        services.AddSingleton<IRedisDatabaseFactory, RedisDatabaseFactory>();
    }
}

[Feature("OrchardCore.Redis.Cache")]
public sealed class RedisCacheStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<IDistributedCache, RedisCacheWrapper>(sp =>
            {
                var optionsAccessor = sp.GetRequiredService<IOptions<RedisCacheOptions>>();
                return new RedisCacheWrapper(new RedisCache(optionsAccessor));
            });
            services.AddTransient<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>();
            services.AddScoped<ITagCache, RedisTagCache>();
        }
    }
}

[Feature("OrchardCore.Redis.Bus")]
public sealed class RedisBusStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<IMessageBus, RedisBus>();
        }
    }
}

[Feature("OrchardCore.Redis.Lock")]
public sealed class RedisLockStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddSingleton<IDistributedLock, RedisLock>();
        }
    }
}

[Feature("OrchardCore.Redis.DataProtection")]
public sealed class RedisDataProtectionStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        if (services.IsRedisConfigured())
        {
            services.AddTransient<IConfigureOptions<KeyManagementOptions>, RedisKeyManagementOptionsSetup>();
        }
    }
}
