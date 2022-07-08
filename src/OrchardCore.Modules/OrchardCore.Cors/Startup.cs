using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Cors.Controllers;
using OrchardCore.Cors.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using CorsService = OrchardCore.Cors.Services.CorsService;

namespace OrchardCore.Cors
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override int Order => -1;

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseCors();

            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "CorsIndex",
                areaName: "OrchardCore.Cors",
                pattern: _adminOptions.AdminUrlPrefix + "/Cors",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddSingleton<CorsService>();

            services.TryAddEnumerable(ServiceDescriptor
                .Transient<IConfigureOptions<CorsOptions>, CorsOptionsConfiguration>());
        }
    }
}
