using Microsoft.AspNetCore.Routing;
using OrchardCore.RateLimits.Core;

namespace OrchardCore.RateLimits.Services;

public sealed class RateLimitRouteNameProvider : IRateLimitRouteNameProvider
{
    private readonly EndpointDataSource _endpointDataSource;

    public RateLimitRouteNameProvider(EndpointDataSource endpointDataSource)
    {
        _endpointDataSource = endpointDataSource;
    }

    public IEnumerable<string> GetRouteNames()
    {
        return GetRoutes()
            .Select(static x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<RateLimitRouteInfo> GetRoutes()
    {
        return _endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(static endpoint =>
            {
                var name = endpoint.Metadata.GetMetadata<IRouteNameMetadata>()?.RouteName
                    ?? endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;

                if (string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }

                return new RateLimitRouteInfo
                {
                    Name = name,
                    Path = FormatPath(endpoint.RoutePattern.RawText),
                    Methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods?
                        .Where(static method => !string.IsNullOrWhiteSpace(method))
                        .Select(static method => method.ToUpperInvariant())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Order(StringComparer.OrdinalIgnoreCase)
                        .ToArray() ?? [],
                };
            })
            .Where(static x => x is not null)
            .DistinctBy(static x => $"{x.Name}:{x.Path}:{string.Join('|', x.Methods)}", StringComparer.OrdinalIgnoreCase)
            .OrderBy(static x => x.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static string FormatPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        return path.StartsWith('/') ? path : "/" + path;
    }
}
