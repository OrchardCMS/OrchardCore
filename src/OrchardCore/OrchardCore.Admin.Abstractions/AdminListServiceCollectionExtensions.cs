using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Admin;

public static class AdminListServiceCollectionExtensions
{
    /// <summary>
    /// Registers a provider that adds, removes or changes the columns of the given admin lists, e.g.
    /// <c>services.AddAdminListColumnProvider&lt;UsersAdminListColumnProvider&gt;(UsersAdminList.Name)</c>. Without a list,
    /// the provider runs for every list and checks <see cref="AdminListColumnsContext.ListName"/> itself.
    /// </summary>
    /// <typeparam name="TProvider">The <see cref="IAdminListColumnProvider"/> implementation to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="listNames">The names of the lists the provider declares columns for, e.g. <c>Users</c>.</param>
    /// <remarks>
    /// A provider registered for a list is only created when that list is rendered. Registering the same provider
    /// twice for a list would run it twice and duplicate the columns it adds, so a registration is only added when
    /// that implementation type is not registered yet for that list.
    /// </remarks>
    public static IServiceCollection AddAdminListColumnProvider<TProvider>(this IServiceCollection services, params string[] listNames)
        where TProvider : class, IAdminListColumnProvider
    {
        if (listNames.Length == 0)
        {
            services.TryAddEnumerable(ServiceDescriptor.Scoped<IAdminListColumnProvider, TProvider>());

            return services;
        }

        foreach (var listName in listNames)
        {
            ArgumentException.ThrowIfNullOrEmpty(listName);

            services.TryAddEnumerable(ServiceDescriptor.KeyedScoped<IAdminListColumnProvider, TProvider>(listName));
        }

        return services;
    }
}
