using Fluid;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Liquid;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    /// <summary>
    /// Adds tenant level services for managing liquid view template files.
    /// </summary>
    public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices(services =>
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


#pragma warning disable CS0618 // Type or member is obsolete
            services.AddLiquidFilter<AppendVersionFilter>("append_version")
                .AddLiquidFilter<ResourceUrlFilter>("resource_url")
                .AddLiquidFilter<SanitizeHtmlFilter>("sanitize_html")

                // Deprecated, remove in a future version.
                .AddLiquidFilter<SupportedCulturesFilter>("supported_cultures");
#pragma warning restore CS0618 // Type or member is obsolete
        });

        return builder;
    }
}
