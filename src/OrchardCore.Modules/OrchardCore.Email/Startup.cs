using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Core;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.Migrations;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Email
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddEmailServices()
                .AddScoped<IDisplayDriver<ISite>, EmailSettingsDisplayDriver>()
                .AddScoped<IPermissionProvider, Permissions>()
                .AddScoped<INavigationProvider, AdminMenu>();

            services.AddDataMigration<EmailMigrations>();
        }
    }
}
