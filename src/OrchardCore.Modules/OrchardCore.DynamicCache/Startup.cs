using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DynamicCache.Services;
using OrchardCore.Environment.Cache;

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
            services.AddScoped<DynamicCacheShapeDisplayEvents>();
            services.AddScoped<IShapeDisplayEvents>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
            services.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
            
            services.AddScoped<IDynamicCacheService, DynamicCacheService>();
            services.AddSingleton<IDynamicCache, DefaultDynamicCache>();
        }
    }
}
