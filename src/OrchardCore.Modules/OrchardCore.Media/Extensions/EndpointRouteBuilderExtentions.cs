using OrchardCore.Media.Endpoints.Api;

namespace Microsoft.AspNetCore.Routing;

public static class EndpointRouteBuilderExtentions
{
    /// <summary>
    /// Adds endpoints for media APIs. 
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/>.</param>
    public static IEndpointRouteBuilder MapMediaApi(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints, nameof(endpoints));

        CreateEndpoints.Map(endpoints);
        GetEndpoints.Map(endpoints);
        DeleteEndpoints.Map(endpoints);
        MoveEndpoints.Map(endpoints);

        return endpoints;
    }
}
