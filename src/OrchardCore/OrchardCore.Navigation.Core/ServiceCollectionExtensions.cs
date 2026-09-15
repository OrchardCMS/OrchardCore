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
    /// Adds the services building the breadcrumb trails of the tenant.
    /// </summary>
    public static IServiceCollection AddBreadcrumbs(this IServiceCollection services)
    {
        services.TryAddScoped<IBreadcrumbManager, BreadcrumbManager>();

        return services;
    }

    /// <summary>
    /// Registers a breadcrumb provider.
    /// </summary>
    /// <typeparam name="TProvider"></typeparam>
    public static IServiceCollection AddBreadcrumbProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IBreadcrumbProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IBreadcrumbProvider, TProvider>());

        return services;
    }
}
