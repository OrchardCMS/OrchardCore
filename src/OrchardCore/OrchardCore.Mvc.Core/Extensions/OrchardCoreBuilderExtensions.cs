using System.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Mvc;
using OrchardCore.Mvc.RazorPages;
using OrchardCore.Routing;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level MVC services and configuration.
        /// </summary>
        public static OrchardCoreBuilder AddMvc(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(collection =>
            {
                // 'PageLoaderMatcherPolicy' doesn't check if an endpoint is a valid candidate.
                // So, we replace it by a custom implementation that does it as other policies.
                var descriptor = collection.FirstOrDefault(d => d.ServiceType == typeof(MatcherPolicy) &&
                    d.ImplementationType?.Name == nameof(PageLoaderMatcherPolicy));

                if (descriptor != null)
                {
                    collection.Remove(descriptor);
                    collection.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, PageLoaderMatcherPolicy>());
                }

                // Allows a tenant to add its own route endpoint schemes for link generation.
                collection.AddSingleton<IEndpointAddressScheme<RouteValuesAddress>, ShellRouteValuesAddressScheme>();
            },
            // Need to be registered last.
            order: int.MaxValue - 100);

            return builder.RegisterStartup<Startup>();
        }
    }
}
