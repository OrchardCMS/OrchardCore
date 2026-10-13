using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Admin;

public static class AdminListServiceCollectionExtensions
{
    /// <summary>
    /// Registers a driver declaring columns of the admin lists of <typeparamref name="TModel"/> items, e.g.
    /// <c>services.AddAdminListColumnDriver&lt;User, UsersAdminListColumnDriver&gt;()</c>. A driver for the
    /// columns of one list only checks the name of the list with
    /// <see cref="AdminListColumnDriver{TModel}.CanHandleList(string)"/>.
    /// </summary>
    /// <typeparam name="TModel">The items of the lists, e.g. <c>User</c>.</typeparam>
    /// <typeparam name="TDriver">The <see cref="IAdminListColumnDriver{TModel}"/> implementation to register.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <remarks>
    /// A driver is only created when a list of <typeparamref name="TModel"/> items is rendered. Registering the
    /// same driver twice would run it twice and duplicate the columns it adds, so a registration is only added
    /// when that implementation type is not registered yet.
    /// </remarks>
    public static IServiceCollection AddAdminListColumnDriver<TModel, TDriver>(this IServiceCollection services)
        where TDriver : class, IAdminListColumnDriver<TModel>
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAdminListColumnDriver<TModel>, TDriver>());

        return services;
    }
}
