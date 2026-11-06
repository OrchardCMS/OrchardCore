using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.OpenId.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class OpenIdClientServiceCollectionExtensions
{
    /// <summary>
    /// Registers a provider of OpenIddict client registrations, e.g. for one of the OpenIddict web providers. The
    /// OpenID Connect client feature registers an authentication scheme and the callback endpoints of each one.
    /// </summary>
    /// <typeparam name="TProvider">The type of the provider, registered as a singleton.</typeparam>
    public static IServiceCollection AddOpenIdClientRegistrationProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IOpenIdClientRegistrationProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IOpenIdClientRegistrationProvider, TProvider>());

        return services;
    }
}
