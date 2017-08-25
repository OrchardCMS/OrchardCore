using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Fluid.Internal;
using Orchard.DisplayManagement.Liquid;
using Orchard.DisplayManagement.Razor;

namespace Orchard.DisplayManagement.Fluid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFluidViews(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FluidViewOptions>,
                FluidViewOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ShapeTemplateOptions>,
                FluidShapeTemplateOptionsSetup>());

            services.TryAddSingleton<IFluidViewFileProviderAccessor, FluidViewFileProviderAccessor>();
            services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, FluidViewsFeatureProvider>();
            services.AddScoped<IRazorViewExtensionProvider, FluidViewExtensionProvider>();
            services.AddSingleton<TagHelperSharedState>();
            return services;
        }
    }
}