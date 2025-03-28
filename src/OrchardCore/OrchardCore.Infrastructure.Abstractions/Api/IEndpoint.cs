using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Infrastructure.Api;

public interface IEndpoint
{
    static abstract IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints);
}
