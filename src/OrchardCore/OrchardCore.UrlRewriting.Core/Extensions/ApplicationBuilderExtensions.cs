using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.UrlRewriting.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>Registers URL rewriting with endpoint reselection for the tenant routing pipeline.</summary>
    public static IApplicationBuilder UseUrlRewriting(this IApplicationBuilder builder, IServiceProvider serviceProvider)
    {
        var rewriteOptions = serviceProvider.GetRequiredService<IOptions<RewriteOptions>>().Value;

        // Orchard selects endpoints before module middleware. Enable ASP.NET Core's native
        // rerouting support using the same tenant route builder. Registering the middleware
        // captures that builder; the temporary property must not affect subsequent modules.
        const string globalRouteBuilderKey = "__GlobalEndpointRouteBuilder";
        var addedRouteBuilder = !builder.Properties.ContainsKey(globalRouteBuilderKey)
            && builder.Properties.TryGetValue("__EndpointRouteBuilder", out var routeBuilder)
            && routeBuilder is IEndpointRouteBuilder;
        if (addedRouteBuilder)
        {
            builder.Properties[globalRouteBuilderKey] = builder.Properties["__EndpointRouteBuilder"];
        }
        try
        {
            builder.UseRewriter(rewriteOptions);
        }
        finally
        {
            if (addedRouteBuilder)
            {
                builder.Properties.Remove(globalRouteBuilderKey);
            }
        }
        return builder;
    }
}
