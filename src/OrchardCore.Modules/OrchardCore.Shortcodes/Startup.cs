using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Shortcodes.Deployment;
using OrchardCore.Shortcodes.Drivers;
using OrchardCore.Shortcodes.Providers;
using OrchardCore.Shortcodes.Recipes;
using OrchardCore.Shortcodes.Services;
using OrchardCore.Shortcodes.ViewModels;
using Sc = Shortcodes;

namespace OrchardCore.Shortcodes;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TemplateOptions>(o =>
        {
            o.MemberAccessStrategy.Register<ShortcodeViewModel>();

            o.MemberAccessStrategy.Register<Sc.Context, object>((obj, name) => obj[name]);

            o.ValueConverters.Add(x =>
            {
                return x switch
                {
                    // Prevent Context from being converted to an ArrayValue as it implements IEnumerable
                    Sc.Context c => new ObjectValue(c),
                    // Prevent Arguments from being converted to an ArrayValue as it implements IEnumerable
                    Sc.Arguments a => new ObjectValue(a),
                    _ => null
                };
            });

            o.MemberAccessStrategy.Register<Sc.Arguments, object>((obj, name) => obj.Named(name));
        });

        services.AddScoped<IShortcodeService, ShortcodeService>();
        services.AddScoped<IShortcodeDescriptorManager, ShortcodeDescriptorManager>();
        services.AddScoped<IShortcodeDescriptorProvider, ShortcodeOptionsDescriptorProvider>();
        services.AddScoped<IShortcodeContextProvider, DefaultShortcodeContextProvider>();

        services.AddOptions<ShortcodeOptions>();
        services.AddScoped<Sc.IShortcodeProvider, OptionsShortcodeProvider>();
        services.AddDisplayDriver<ShortcodeDescriptor, ShortcodeDescriptorDisplayDriver>();
    }
}

[Feature("OrchardCore.Shortcodes.Templates")]
public sealed class ShortcodeTemplatesStartup : StartupBase
{
    // Register this first so the templates provide overrides for any code driven shortcodes.
    public override int Order => -10;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ShortcodeTemplatesManager>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();

        services.AddRecipeExecutionStep<ShortcodeTemplateStep>();

        services.AddScoped<Sc.IShortcodeProvider, TemplateShortcodeProvider>();
        services.AddScoped<IShortcodeDescriptorProvider, ShortcodeTemplatesDescriptorProvider>();
    }
}

[RequireFeatures("OrchardCore.Localization")]
public sealed class LocaleShortcodeProviderStartup : StartupBase
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
            d.Categories = ["Localization"];
        });
    }
}

[RequireFeatures("OrchardCore.Deployment", "OrchardCore.Shortcodes.Templates")]
public sealed class ShortcodeTemplatesDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllShortcodeTemplatesDeploymentSource, AllShortcodeTemplatesDeploymentStep, AllShortcodeTemplatesDeploymentStepDriver>();
    }
}
