using System;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes;
using OrchardCore.Shortcodes.Controllers;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using Shortcodes;

namespace OrchardCore.Shortcodes
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ShortcodeViewModel>();

            TemplateContext.GlobalMemberAccessStrategy.Register<Arguments, object>((obj, name) => obj.NamedOrDefault(name));

            // Prevent Arguments from being converted to an ArrayValue as they implement IEnumerable
            FluidValue.SetTypeMapping<Arguments>(o => new ObjectValue(o));
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortcodeService, ShortcodeService>();
            services.AddScoped<IShortcodeTableManager, ShortcodeTableManager>();
            services.AddScoped<IShortcodeTableProvider, ShortcodeOptionsTableProvider>();

            services.AddOptions<ShortcodeOptions>();
            services.AddScoped<IShortcodeProvider, OptionsShortcodeProvider>();

            //TODo testing code remove.
            services.AddShortcode("bold", (args, content) => {
                var text = args.Named("text");
                if (!String.IsNullOrEmpty(text))
                {
                    content = text;
                }

                return new ValueTask<string>($"<em>{content}</em>");
            });

            services.AddShortcode("bold", (args, content) => {
                var text = args.NamedOrDefault("text");
                if (!String.IsNullOrEmpty(text))
                {
                    content = text;
                }

                //TODO when we have the sanitizer we will put it in the context by default (IDefaultShortcodeContextProvider)
                // so that a delegate such as this can easily sanitize it's input.
                return new ValueTask<string>($"<b>{content}</b>");

            }, d => {
                d.DefaultShortcode = "[bold]";

                //d.DefaultContent = // none in this case
                // TODO this is going to give no end of escaping problems
                d.Hint = "Add bold formatting with a shortcode.<br>Usage: [bold 'your bold content here]'";
                d.Categories = new string[] { "HTML Content" };

            });
        }

    }

    [Feature("OrchardCore.Shortcodes.Templates")]
    public class ShortcodeTemplatesStartup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public ShortcodeTemplatesStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        // Register this first so the templates provide overrides for any code driven shortcodes.
        public override int Order => -10;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ShortcodeTemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<IShortcodeProvider, TemplateShortcodeProvider>();
            services.AddScoped<IShortcodeTableProvider, ShortcodeTemplatesTableProvider>();
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
