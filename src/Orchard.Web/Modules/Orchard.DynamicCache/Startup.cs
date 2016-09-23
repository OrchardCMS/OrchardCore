using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Implementation;
using Orchard.DynamicCache.Services;
using Orchard.Environment.Cache.Abstractions;

namespace Orchard.DynamicCache
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Module : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<DynamicCacheShapeDisplayEvents>();
            services.AddScoped<IShapeDisplayEvents>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
            services.AddScoped<ITagRemovedEventHandler>(sp => sp.GetRequiredService<DynamicCacheShapeDisplayEvents>());
        }
    }
}
