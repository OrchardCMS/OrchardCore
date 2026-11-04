using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Email.Endpoints;
using OrchardCore.RemoteManagement;
using OrchardCore.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Drivers;
using OrchardCore.Email.Migrations;
using OrchardCore.Email.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Email;

public sealed class Startup : StartupBase
{
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddEmailManagementEndpoints();
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ISiteSettingsSectionProvider, EmailSettingsSectionProvider>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, EmailCapabilityProvider>();
        services.AddEmailServices()
            .AddSignalOptionsChangeTokenSource<EmailOptions>()
            .AddSignalOptionsChangeTokenSource<EmailProviderOptions>()
            .AddSiteDisplayDriver<EmailSettingsDisplayDriver>()
            .AddSiteSettingsPermission(EmailSettings.GroupId, EmailPermissions.ManageEmailSettings)
            .AddPermissionProvider<Permissions>()
            .AddNavigationProvider<AdminMenu>();

        services.AddDataMigration<EmailMigrations>();
    }
}
