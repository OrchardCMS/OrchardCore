using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.DynamicCache.Services;
using OrchardCore.Modules;

namespace OrchardCore.DynamicCache
{
    [Feature(FeatureName)]
    public class AntiforgeryStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll<IDynamicCacheService>();
            services.AddScoped<IDynamicCacheService>(sp => new AntiForgeryDynamicCacheService(
                sp.GetRequiredService<DefaultDynamicCacheService>(),
                sp.GetRequiredService<IAntiforgery>(),
                sp.GetRequiredService<IHttpContextAccessor>()
            ));
        }

        internal const string FeatureName = "OrchardCore.DynamicCache.Antiforgery";
    }
}