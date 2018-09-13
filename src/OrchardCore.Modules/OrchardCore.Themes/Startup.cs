using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Themes.Deployment;
using OrchardCore.Themes.Recipes;
using OrchardCore.Themes.Services;

namespace OrchardCore.Themes
{
    /// <summary>
    /// These services are registered on the tenant service collection
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

            services.AddTransient<IDeploymentSource, ThemesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<ThemesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, ThemesDeploymentStepDriver>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
