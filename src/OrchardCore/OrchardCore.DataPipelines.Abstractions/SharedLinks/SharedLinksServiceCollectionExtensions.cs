using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.DataPipelines.SharedLinks;

/// <summary>
/// Registers the handlers of shared links.
/// </summary>
public static class SharedLinksServiceCollectionExtensions
{
    /// <summary>
    /// Registers a handler that opens the links to a kind of resource. The links themselves come with the
    /// <c>OrchardCore.DataPipelines.Core</c> feature.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler.</typeparam>
    /// <param name="services">The services.</param>
    /// <returns>The services.</returns>
    public static IServiceCollection AddSharedLinkHandler<THandler>(this IServiceCollection services)
        where THandler : class, ISharedLinkHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<ISharedLinkHandler, THandler>());

        return services;
    }
}
