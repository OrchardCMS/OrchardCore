using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Modules;

public abstract class StartupBase : IStartup, IAsyncStartup
{
    /// <inheritdoc />
    public virtual int Order { get; } = OrchardCoreConstants.ConfigureOrder.Default;

    /// <inheritdoc />
    public virtual int ConfigureOrder => Order;

    /// <inheritdoc />
    public virtual void ConfigureServices(IServiceCollection services)
    {
    }

    /// <inheritdoc />
    public virtual void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }

    /// <inheritdoc />
    public virtual ValueTask ConfigureAsync(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider) => default;
}
