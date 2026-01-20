using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Routing
{
    /// <summary>
    /// Marker interface to retrieve tenant 'RouteValuesAddress' schemes used for link generation.
    /// </summary>
    public interface IShellRouteValuesAddressScheme : IEndpointAddressScheme<RouteValuesAddress>
    {
    }
}
