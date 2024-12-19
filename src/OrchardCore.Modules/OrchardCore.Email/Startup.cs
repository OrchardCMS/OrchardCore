using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Core;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.Migrations;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddEmailServices()
            .AddSiteDisplayDriver<EmailSettingsDisplayDriver>()
            .AddPermissionProvider<Permissions>()
            .AddNavigationProvider<AdminMenu>();

        services.AddDataMigration<EmailMigrations>();
    }
}
