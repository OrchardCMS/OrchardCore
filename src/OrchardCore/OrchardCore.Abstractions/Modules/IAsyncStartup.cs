using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Modules;

/// <summary>
/// An implementation of this interface allows to configure asynchronously the tenant pipeline.
/// </summary>
public interface IAsyncStartup
{
    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the tenant pipeline.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="routes"></param>
    /// <param name="serviceProvider"></param>
    ValueTask ConfigureAsync(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider);
}
