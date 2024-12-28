using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
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
    }
}
