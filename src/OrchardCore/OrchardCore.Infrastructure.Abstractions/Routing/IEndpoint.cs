using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Routing;

public interface IEndpoint
{
    void Map(IEndpointRouteBuilder builder);
}
