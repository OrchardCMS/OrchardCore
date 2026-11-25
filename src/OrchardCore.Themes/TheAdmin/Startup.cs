using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Recipes;
using OrchardCore.Themes.Models;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Drivers;
using OrchardCore.Themes.TheAdmin.Migrations;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Recipes;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<Navbar, ToggleThemeNavbarDisplayDriver>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        // The schemes: imported once by a migration, managed from the theme's card on Design > Themes, and picked by each
        // user from the user menu.
        services.AddScoped<AdminSchemeStore>();
        services.AddScoped<AdminSchemeService>();
        services.AddDisplayDriver<AdminSchemeEntry, AdminSchemeDisplayDriver>();
        services.AddDisplayDriver<ThemeEntry, ThemeEntrySchemesDisplayDriver>();
        services.AddRecipeExecutionStep<AdminSchemesStep>();
        services.AddDataMigration<AdminSchemesMigrations>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAdminSchemeStylesheet();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AdminSchemesDeploymentSource, AdminSchemesDeploymentStep, AdminSchemesDeploymentStepDriver>();
    }
}
