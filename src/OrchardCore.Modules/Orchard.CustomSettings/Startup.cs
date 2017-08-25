using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.CustomSettings.Services;
using Orchard.DisplayManagement.Handlers;
using Orchard.Environment.Navigation;
using Orchard.Layers.Drivers;
using Orchard.Security.Permissions;
using Orchard.Settings;

namespace Orchard.CustomSettings
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, CustomSettingsDisplayDriver>();
            services.AddScoped<CustomSettingsService>();

            // Permissions
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, CustomSettingsAuthorizationHandler>();
        }
    }
}
