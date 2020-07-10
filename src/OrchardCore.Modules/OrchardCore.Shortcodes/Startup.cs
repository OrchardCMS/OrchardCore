using System;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes.Controllers;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Shortcodes
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<Arguments, object>((obj, name) => obj.NamedOrDefault(name));

            // Prevent Arguments from being converted to an ArrayValue as they implement IEnumerable
            FluidValue.SetTypeMapping<Arguments>(o => new ObjectValue(o));
        }
        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortcodeService, ShortcodeService>();


            services.AddScoped<ShortcodeTemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IShortcodeProvider, TemplateShortcodeProvider>();

        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var templateControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Shortcodes.Index",
                areaName: "OrchardCore.Shortcodes",
                pattern: _adminOptions.AdminUrlPrefix + "/Shortcodes",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Shortcodes.Create",
                areaName: "OrchardCore.Shortcodes",
                pattern: _adminOptions.AdminUrlPrefix + "/Shortcodes/Create",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Shortcodes.Edit",
                areaName: "OrchardCore.Shortcodes",
                pattern: _adminOptions.AdminUrlPrefix + "/Shortcodes/Edit/{name}",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Edit) }
            );
        }
    }
}
