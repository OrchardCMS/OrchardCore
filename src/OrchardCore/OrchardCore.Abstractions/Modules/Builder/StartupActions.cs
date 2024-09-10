using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.Extensions.DependencyInjection;

internal sealed class StartupActions
{
    public StartupActions(int order)
    {
        Order = order;
    }

    public int Order { get; }

    public ICollection<Action<IServiceCollection, IServiceProvider>> ConfigureServicesActions { get; } = [];

    public ICollection<Action<IApplicationBuilder, IEndpointRouteBuilder, IServiceProvider>> ConfigureActions { get; } = [];

    public ICollection<Func<IApplicationBuilder, IEndpointRouteBuilder, IServiceProvider, ValueTask>> AsyncConfigureActions { get; } = [];
}
