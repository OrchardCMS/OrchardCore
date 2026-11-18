using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Themes.Deployment;
using OrchardCore.Themes.Drivers;
using OrchardCore.Themes.Models;
using OrchardCore.Themes.Recipes;
using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.Schemes.Deployment;
using OrchardCore.Themes.Schemes.Drivers;
using OrchardCore.Themes.Schemes.Models;
using OrchardCore.Themes.Schemes.Recipes;
using OrchardCore.Themes.Schemes.Services;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();
        services.AddRecipeExecutionStep<ThemesStep>();
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IThemeSelector, SiteThemeSelector>();
        services.AddScoped<ISiteThemeService, SiteThemeService>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<ThemeTogglerService>();
        services.AddDeployment<ThemesDeploymentSource, ThemesDeploymentStep, ThemesDeploymentStepDriver>();
        services.AddDisplayDriver<ThemeEntry, ThemeEntryDisplayDriver>();
        services.AddShapeTableProvider<AdminDashboardShapeTableProvider>();
    }
}

/// <summary>
/// The services of the Theme Schemes feature, which the themes that support schemes depend on.
/// </summary>
[Feature(ThemeSchemesConstants.FeatureId)]
public sealed class SchemesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<ThemeSchemesOptions>();
        services.AddScoped<ThemeSchemeStore>();
        services.AddScoped<ThemeSchemeService>();
        services.AddDisplayDriver<ThemeSchemeEntry, ThemeSchemeDisplayDriver>();
        services.AddDisplayDriver<ThemeEntry, ThemeEntrySchemesDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapThemeSchemeStylesheets();
    }
}

[Feature(ThemeSchemesConstants.FeatureId)]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class SchemesDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<ThemeSchemesDeploymentSource, ThemeSchemesDeploymentStep, ThemeSchemesDeploymentStepDriver>();
    }
}

[Feature(ThemeSchemesConstants.FeatureId)]
[RequireFeatures("OrchardCore.Recipes.Core")]
public sealed class SchemesRecipesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddRecipeExecutionStep<ThemeSchemesStep>();
    }
}
