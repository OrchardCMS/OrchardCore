using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DynamicCache.EventHandlers;
using OrchardCore.DynamicCache.Services;
using OrchardCore.DynamicCache.TagHelpers;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.DynamicCache
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDynamicCacheService, DefaultDynamicCacheService>();
            services.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<IDynamicCacheService>());

            services.AddScoped<IShapeDisplayEvents, DynamicCacheShapeDisplayEvents>();
            services.AddShapeAttributes<CachedShapeWrapperShapes>();

            services.AddSingleton<IDynamicCache, DefaultDynamicCache>();
            services.AddSingleton<DynamicCacheTagHelperService>();
            services.AddTagHelpers<DynamicCacheTagHelper>();
            services.AddTagHelpers<CacheDependencyTagHelper>();

            services.AddTransient<IConfigureOptions<CacheOptions>, CacheOptionsConfiguration>();
            services.Configure<DynamicCacheOptions>(_shellConfiguration.GetSection("OrchardCore_DynamicCache"));
        }
    }
}
