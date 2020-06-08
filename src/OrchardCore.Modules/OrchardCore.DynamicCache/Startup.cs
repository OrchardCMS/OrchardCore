using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DynamicCache.EventHandlers;
using OrchardCore.DynamicCache.Services;
using OrchardCore.DynamicCache.TagHelpers;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;

namespace OrchardCore.DynamicCache
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Register the type as it implements multiple interfaces
            services.AddScoped<DefaultDynamicCacheService>();
            services.AddScoped<IDynamicCacheService>(sp => sp.GetRequiredService<DefaultDynamicCacheService>());
            services.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<DefaultDynamicCacheService>());

            services.AddScoped<IShapeDisplayEvents, DynamicCacheShapeDisplayEvents>();

            services.AddShapeAttributes<CachedShapeWrapperShapes>();

            services.AddSingleton<IDynamicCache, DefaultDynamicCache>();
            services.AddSingleton<DynamicCacheTagHelperService>();
            services.AddTagHelpers<DynamicCacheTagHelper>();
        }
    }
}
