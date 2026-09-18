using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Navigation;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds tenant level services.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddNavigation(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<INavigationManager, NavigationManager>());

        return services;
    }

    /// <summary>
    /// Registers a navigation provider.
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    public static IServiceCollection AddNavigationProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, INavigationProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<INavigationProvider, TProvider>());

        return services;
    }

    /// <summary>
    /// Registers a scoped provider that updates breadcrumb items after their inline declarations.
    /// </summary>
    /// <typeparam name="TProvider">The breadcrumb provider implementation.</typeparam>
    /// <param name="services">The tenant service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddBreadcrumbProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IBreadcrumbProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IBreadcrumbProvider, TProvider>());

        return services;
    }
}
