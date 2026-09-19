using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Admin;

public static class AdminListServiceCollectionExtensions
{
    /// <summary>
    /// Registers a provider that can add, remove or reorder the columns of an admin list.
    /// </summary>
    /// <typeparam name="TProvider">The <see cref="IAdminListColumnProvider"/> implementation to register.</typeparam>
    /// <remarks>
    /// Registering the same provider twice would run it twice and duplicate the columns it adds, so the
    /// registration is only added when that implementation type is not registered yet.
    /// </remarks>
    public static IServiceCollection AddAdminListColumnProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IAdminListColumnProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAdminListColumnProvider, TProvider>());

        return services;
    }
}
