using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Settings.Deployment;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Drivers;
using OrchardCore.Themes.TheAdmin.Migrations;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Navigation;
using OrchardCore.Themes.TheAdmin.Recipes;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<Navbar, ToggleThemeNavbarDisplayDriver>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        services.AddScoped<AdminSchemeStore>();
        services.AddScoped<AdminSchemeService>();
        services.AddSiteDisplayDriver<AdminThemeSettingsDisplayDriver>();
        services.AddDisplayDriver<AdminScheme, AdminSchemeDisplayDriver>();
        services.AddNavigationProvider<AdminSchemesAdminMenu>();
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
        services.AddSiteSettingsPropertyDeploymentStep<AdminThemeSettings, DeploymentStartup>(S => S["Admin theme settings"], S => S["Exports the default scheme of the admin theme."]);
        services.AddDeployment<AdminSchemesDeploymentSource, AdminSchemesDeploymentStep, AdminSchemesDeploymentStepDriver>();
    }
}

[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class RecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<AdminSchemesStep>();
    }
}
