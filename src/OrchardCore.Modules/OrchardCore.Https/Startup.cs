using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Navigation;
using OrchardCore.Https.Drivers;
using OrchardCore.Https.Services;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Https
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, HttpsSettingsDisplayDriver>();
            services.AddSingleton<IHttpsService, HttpsService>();

            services.TryAddEnumerable(ServiceDescriptor.Transient<IConfigureOptions<MvcOptions>, MvcOptionsHttpsConfiguration>());
        }
    }
}
