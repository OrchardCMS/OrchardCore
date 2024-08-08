using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc;
using OrchardCore.Routing;

namespace Microsoft.Extensions.DependencyInjection;

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
                // The Cors module is designed to handle CORS, thus we skip checking for unhandled security metadata by default.
                // Additionally, skipping security metadata checks on the endpoint provides a minor performance benefit.
                options.SuppressCheckForUnhandledSecurityMetadata = true;
            });
        },
        // Need to be registered last.
        order: int.MaxValue - 100);

        return builder.RegisterStartup<Startup>();
    }
}
