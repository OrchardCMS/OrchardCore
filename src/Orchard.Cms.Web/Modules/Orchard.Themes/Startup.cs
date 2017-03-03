using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Navigation;
using Orchard.Recipes;
using Orchard.Security.Permissions;
using Orchard.Themes.Recipes;
using Orchard.Themes.Services;

namespace Orchard.Themes
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
            services.AddScoped<IThemeSelector, SafeModeThemeSelector>();
            services.AddScoped<IThemeSelector, SiteThemeSelector>();
            services.AddScoped<ISiteThemeService, SiteThemeService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
