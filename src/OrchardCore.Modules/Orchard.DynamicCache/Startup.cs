using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Implementation;
using Orchard.DynamicCache.Services;
using Orchard.Environment.Cache;

namespace Orchard.DynamicCache
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<DynamicCacheShapeDisplayEvents>();
            services.AddScoped<IShapeDisplayEvents>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
            services.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());

            services.AddSingleton<IDynamicCache, DefaultDynamicCache>();
        }
    }
}
