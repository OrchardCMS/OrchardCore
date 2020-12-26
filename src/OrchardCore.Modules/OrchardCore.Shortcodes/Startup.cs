using System;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes.Controllers;
using OrchardCore.Shortcodes.Deployment;
using OrchardCore.Shortcodes.Drivers;
using OrchardCore.Shortcodes.Providers;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using Shortcodes;
using Sc = Shortcodes;

namespace OrchardCore.Shortcodes
{
    public class Startup : StartupBase
    {
        static Startup()
        {
            TemplateContext.GlobalMemberAccessStrategy.Register<ShortcodeViewModel>();

            TemplateContext.GlobalMemberAccessStrategy.Register<Context, object>((obj, name) => obj[name]);

            // Prevent Context from being converted to an ArrayValue as it implements IEnumerable
            FluidValue.SetTypeMapping<Context>(o => new ObjectValue(o));

            TemplateContext.GlobalMemberAccessStrategy.Register<Sc.Arguments, object>((obj, name) => obj.NamedOrDefault(name));

            // Prevent Arguments from being converted to an ArrayValue as it implements IEnumerable
            FluidValue.SetTypeMapping<Sc.Arguments>(o => new ObjectValue(o));
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IShortcodeService, ShortcodeService>();
            services.AddScoped<IShortcodeDescriptorManager, ShortcodeDescriptorManager>();
            services.AddScoped<IShortcodeDescriptorProvider, ShortcodeOptionsDescriptorProvider>();
            services.AddScoped<IShortcodeContextProvider, DefaultShortcodeContextProvider>();

            services.AddOptions<ShortcodeOptions>();
            services.AddScoped<IShortcodeProvider, OptionsShortcodeProvider>();
            services.AddScoped<IDisplayManager<ShortcodeDescriptor>, DisplayManager<ShortcodeDescriptor>>();
            services.AddScoped<IDisplayDriver<ShortcodeDescriptor>, ShortcodeDescriptorDisplayDriver>();
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
            services.AddTransient<IDeploymentSource, AllShortcodeTemplatesDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllShortcodeTemplatesDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllShortcodeTemplatesDeploymentStepDriver>();

            services.AddScoped<ShortcodeTemplatesManager>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddScoped<IShortcodeProvider, TemplateShortcodeProvider>();
            services.AddScoped<IShortcodeDescriptorProvider, ShortcodeTemplatesDescriptorProvider>();
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

    [RequireFeatures("OrchardCore.Localization")]
    public class LocaleShortcodeProviderStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddShortcode<LocaleShortcodeProvider>("locale", d =>
            {
                d.DefaultValue = "[locale {language_code}] [/locale]";
                d.Hint = "Conditionally render content in the specified language";
                d.Usage =
@"[locale en]English Text[/locale][locale fr false]French Text[/locale]<br>
<table>
  <tr>
    <td>Args:</td>
    <td>lang, fallback</td>
  </tr>
</table>";
                d.Categories = new string[] { "Localization" };
            });
        }
    }
}
