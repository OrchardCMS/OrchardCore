using Fluid;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Razor;

namespace Microsoft.Extensions.DependencyInjection;

public static class LiquidCoreServices
{
    public static IServiceCollection AddLiquidCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<LiquidViewParser>();
        services.AddSingleton<IAnchorTag, AnchorTag>();

        services.AddTransient<IConfigureOptions<TemplateOptions>, TemplateOptionsFileProviderSetup>();

        services.AddTransient<IConfigureOptions<LiquidViewOptions>, LiquidViewOptionsSetup>();

        services.AddTransient<IConfigureOptions<ShapeTemplateOptions>, LiquidShapeTemplateOptionsSetup>();

        services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, LiquidViewsFeatureProvider>();
        services.AddScoped<IRazorViewExtensionProvider, LiquidViewExtensionProvider>();
        services.AddSingleton<LiquidTagHelperFactory>();

        services.AddSingleton<IConfigureOptions<TemplateOptions>, TemplateOptionsConfigurations>();

        return services;
    }
}
