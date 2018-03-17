using OrchardCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DynamicCache.EventHandlers;
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
            services.AddScoped<IDynamicCacheService, DefaultDynamicCacheService>();
            services.AddScoped<IContentDisplayHandler, DynamicCacheContentDisplayHandler>();

            // Register the type as it implements multiple interfaces
            services.AddScoped<DynamicCacheShapeDisplayEvents>();
            services.AddScoped<IShapeDisplayEvents>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
            services.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
            
            services.AddSingleton<IDynamicCache, DefaultDynamicCache>();
        }
    }
}
