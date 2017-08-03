using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using Orchard.DisplayManagement.Fluid.Internal;
using Orchard.DisplayManagement.Razor;

namespace Orchard.DisplayManagement.Fluid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFluidViews(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FluidViewOptions>, FluidViewOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>,
                FluidViewRazorOptionsSetup<RazorViewEngineOptions>>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<ShapeTemplateOptions>,
                FluidViewRazorOptionsSetup<ShapeTemplateOptions>>());

            services.TryAddSingleton<IFluidViewFileProviderAccessor, FluidViewFileProviderAccessor>();
            services.TryAddSingleton<IFluidViewRazorFileProvider, FluidViewRazorFileProvider>();
            services.AddScoped<IApplicationFeatureProvider<ViewsFeature>, FluidViewsFeatureProvider>();
            services.AddScoped<IRazorViewExtensionProvider, FluidViewExtensionProvider>();

            return services;
        }
    }
}