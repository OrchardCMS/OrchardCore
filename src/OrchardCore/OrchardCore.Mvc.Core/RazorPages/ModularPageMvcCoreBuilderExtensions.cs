using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc.RazorPages
{
    public static class ModularPageMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddModularRazorPages(this IMvcCoreBuilder builder)
        {
            builder.AddRazorPages();
            builder.Services.AddModularRazorPages();
            return builder;
        }

        internal static IServiceCollection AddModularRazorPages(this IServiceCollection services)
        {
            // 'PageLoaderMatcherPolicy' doesn't check if an endpoint is a valid candidate.
            // So, we replace it by a custom implementation that does it as other policies.
            var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(MatcherPolicy) &&
                d.ImplementationType?.Name == nameof(PageLoaderMatcherPolicy));

            if (descriptor != null)
            {
                services.Remove(descriptor);
                services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, PageLoaderMatcherPolicy>());
            }

            services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, PageEndpointComparerPolicy>());

            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorPagesOptions>, ModularPageRazorPagesOptionsSetup>());

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPageApplicationModelProvider, ModularPageApplicationModelProvider>());

            return services;
        }
    }
}
