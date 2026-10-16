using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Admin;

public static class AdminListServiceCollectionExtensions
{
    /// <summary>
    /// Registers a provider extending view-defined columns. Registering the same provider twice has no effect.
    /// </summary>
    /// <typeparam name="TProvider">The provider implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public static IServiceCollection AddAdminListColumnsProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IAdminListColumnsProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAdminListColumnsProvider, TProvider>());

        return services;
    }
}
