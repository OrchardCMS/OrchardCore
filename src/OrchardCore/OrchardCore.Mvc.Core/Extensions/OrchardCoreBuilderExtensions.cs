using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc;
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
                // Allows a tenant to add its own route endpoint schemes for link generation.
                collection.AddSingleton<IEndpointAddressScheme<RouteValuesAddress>, ShellRouteValuesAddressScheme>();

                collection.Configure<RouteOptions>(options =>
                {
                    // The Cors module is designed to deal with CORS, so we skip checking for unhandled security metadata by default.
                    // In addition to a small performance benefit to skip checking for security metadata on endpoint.
                    options.SuppressCheckForUnhandledSecurityMetadata = true;
                });
            },
            // Need to be registered last.
            order: int.MaxValue - 100);

            return builder.RegisterStartup<Startup>();
        }
    }
}
