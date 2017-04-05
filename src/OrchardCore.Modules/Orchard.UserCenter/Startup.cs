using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Navigation;
using Orchard.Security.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.UserCenter
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddNavigation();

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(UserCenterZoneFilter));
                options.Filters.Add(typeof(UserCenterFilter));
                options.Filters.Add(typeof(UserCenterMenuFilter));
            });

            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IThemeSelector, UserCenterThemeSelector>();
            services.AddScoped<IUserCenterThemeService, UserCenterThemeService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapAreaRoute(
                name: "Orchard.UserCenter",
                areaName: "Orchard.UserCenter",
                template: "usercenter",
                defaults: new { controller = "UserCenter", action = "Index" }
            );
        }
    }
}
