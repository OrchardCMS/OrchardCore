using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Routing
{
    /// <summary>
    /// Marker interface to retrieve tenant 'RouteValuesAddress' schemes used for link generation.
    /// </summary>
    public interface IShellRouteValuesAddressScheme : IEndpointAddressScheme<RouteValuesAddress>
    {
    }
}