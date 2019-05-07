using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Liquid.Internal;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.Liquid;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing liquid view template files.
        /// </summary>
        public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<LiquidViewOptions>,
                LiquidViewOptionsSetup>());

                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<ShapeTemplateOptions>,
                    LiquidShapeTemplateOptionsSetup>());

                services.TryAddSingleton<ILiquidViewFileProviderAccessor, LiquidViewFileProviderAccessor>();
                services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, LiquidViewsFeatureProvider>();
                services.AddScoped<IRazorViewExtensionProvider, LiquidViewExtensionProvider>();
                services.AddSingleton<LiquidTagHelperFactory>();

                services.AddScoped<ILiquidTemplateEventHandler, RequestLiquidTemplateEventHandler>();
                services.AddScoped<ILiquidTemplateEventHandler, CultureLiquidTemplateEventHandler>();

                services.AddLiquidFilter<AppendVersionFilter>("append_version");
            });

            return builder;
        }
    }
}