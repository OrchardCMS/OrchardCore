using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Modules;
using OrchardCore.SecureSocketsLayer.Drivers;
using OrchardCore.SecureSocketsLayer.Filters;
using OrchardCore.SecureSocketsLayer.Services;
using OrchardCore.Settings;

namespace OrchardCore.SecureSocketsLayer
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(SecureSocketsLayersFilter));
            });

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, SslSettingsDisplayDriver>();
            services.AddScoped<ISecureSocketsLayerService, SecureSocketsLayerService>();
        }
    }
}
