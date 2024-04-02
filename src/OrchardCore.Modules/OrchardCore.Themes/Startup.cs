using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin.Models;
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

namespace OrchardCore.Themes
{
    /// <summary>
    /// These services are registered on the tenant service collection.
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRecipeExecutionStep<ThemesStep>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IThemeSelector, SiteThemeSelector>();
            services.AddScoped<ISiteThemeService, SiteThemeService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IThemeService, ThemeService>();
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddScoped<DarkModeService>();
#pragma warning restore CS0618 // Type or member is obsolete
            services.AddScoped<ThemeTogglerService>();
            services.AddDeployment<ThemesDeploymentSource, ThemesDeploymentStep, ThemesDeploymentStepDriver>();
            services.AddScoped<IDisplayDriver<Navbar>, ToggleThemeNavbarDisplayDriver>();
            services.AddScoped<IDisplayDriver<ThemeEntry>, ThemeEntryDisplayDriver>();
        }
    }
}
