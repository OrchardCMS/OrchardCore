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

            // Replaces the framework's endpoint name address scheme with one that tolerates the
            // duplicate endpoint names produced by dynamic controller routes. When such a route is
            // mapped (e.g. Autoroute, HomeRoute, Sitemaps), the shared controller endpoint data source
            // also emits a second, non-routable placeholder endpoint for every action that copies the
            // action's '[EndpointName]', so attribute-routed API controllers end up with two endpoints
            // sharing the same name. The default scheme throws on such duplicates during any link
            // generation by name; this one ignores the non-routable placeholder while still throwing
            // when two real routable endpoints genuinely share a name.
            collection.AddSingleton<IEndpointAddressScheme<string>, ShellEndpointNameAddressScheme>();

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
