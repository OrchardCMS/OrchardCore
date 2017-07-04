using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Orchard.DisplayManagement.Fluid.Internal;

namespace Orchard.DisplayManagement.Fluid
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFluidViews(this IServiceCollection services)
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<FluidViewOptions>, FluidViewOptionsSetup>());

            services.TryAddSingleton<IFluidViewFileProviderAccessor, FluidViewFileProviderAccessor>();
            services.AddScoped<IApplicationFeatureProvider<ViewsFeature>, FluidViewsFeatureProvider>();

            return services;
        }
    }
}