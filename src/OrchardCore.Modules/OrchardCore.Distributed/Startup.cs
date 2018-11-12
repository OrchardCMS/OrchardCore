using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Distributed.Core.Services;
using OrchardCore.Distributed.Redis;
using OrchardCore.Distributed.Redis.DataProtection;
using OrchardCore.Distributed.Redis.Drivers;
using OrchardCore.Distributed.Redis.Options;
using OrchardCore.Distributed.Redis.Services;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Distributed
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [Feature("OrchardCore.Distributed.Signal")]
    public class DistributedSignalStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<DistributedSignal>();
            services.AddSingleton<ISignal>(sp => sp.GetRequiredService<DistributedSignal>());
            services.AddSingleton<IModularTenantEvents>(sp => sp.GetRequiredService<DistributedSignal>());
        }
    }

    [Feature("OrchardCore.Distributed.Shell")]
    public class DistributedShellStartup : StartupBase
    {
        private readonly ShellSettings _shellSettings;

        public DistributedShellStartup(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (_shellSettings.Name == ShellHelper.DefaultShellName)
            {
                services.AddSingleton<IShellEvents, DistributedShell>();
            }
        }
    }

    [Feature("OrchardCore.Distributed.Redis")]
    public class RedisStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, RedisPermissions>();
            services.AddScoped<INavigationProvider, RedisAdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, RedisSiteSettingsDisplayDriver>();

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RedisOptions>, RedisOptionsSetup>());

            services.AddSingleton<IRedisClient, RedisClient>();
        }
    }

    [Feature("OrchardCore.Distributed.Redis.Cache")]
    public class RedisCacheStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RedisCacheOptions>, RedisCacheOptionsSetup>());

            services.AddDistributedRedisCache(o => { });

            services.AddTransient<ITagCache, RedisTagCache>();
        }
    }

    [Feature("OrchardCore.Distributed.Redis.Bus")]
    public class RedisBusStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMessageBus, RedisBus>();
        }
    }

    [Feature("OrchardCore.Distributed.Redis.Lock")]
    public class RedisLockStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ILock, RedisLock>();
        }
    }

    [Feature("OrchardCore.Distributed.Redis.DataProtection")]
    public class RedisDataProtectionStartup : StartupBase
    {
        private readonly ShellSettings _shellSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RedisDataProtectionStartup(ShellSettings shellSettings, IHttpContextAccessor httpContextAccessor)
        {
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        public override int Order => 10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new RedisXmlRepository(() => 
                    _httpContextAccessor.HttpContext.RequestServices.GetService<IRedisClient>().Database,
                    _shellSettings.Name + ":DataProtection-Keys");
            });
        }
    }
}
