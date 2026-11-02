using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Admin;

public static class AdminListServiceCollectionExtensions
{
    /// <summary>
    /// Registers a provider extending every list's view-defined columns, before list-specific providers run.
    /// Registering the same global provider twice has no effect.
    /// </summary>
    /// <typeparam name="TProvider">The provider implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public static IServiceCollection AddAdminListColumnsProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IAdminListColumnsProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAdminListColumnsProvider, TProvider>());

        return services;
    }

    /// <summary>
    /// Registers a provider extending one list's view-defined columns. Registering the same provider
    /// for the same list twice has no effect. Multiple providers can extend the same list, after global providers run.
    /// </summary>
    /// <typeparam name="TProvider">The provider implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="listName">The case-sensitive list name declared by the rendering view.</param>
    public static IServiceCollection AddAdminListColumnsProvider<TProvider>(this IServiceCollection services, string listName)
        where TProvider : class, IAdminListColumnsProvider
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);
        services.TryAddEnumerable(ServiceDescriptor.KeyedScoped<IAdminListColumnsProvider, TProvider>(new AdminListColumnsProviderKey(listName)));

        return services;
    }
}
