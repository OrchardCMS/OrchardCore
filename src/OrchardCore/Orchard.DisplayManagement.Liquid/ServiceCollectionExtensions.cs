using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Liquid.Internal;
using Orchard.DisplayManagement.Razor;

namespace Orchard.DisplayManagement.Liquid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLiquidViews(this IServiceCollection services)
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
            services.AddSingleton<TagHelperSharedState>();
            return services;
        }
    }
}