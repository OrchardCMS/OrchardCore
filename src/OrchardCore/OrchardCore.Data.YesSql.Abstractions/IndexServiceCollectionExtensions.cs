using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YesSql.Indexes;

namespace OrchardCore.Data;

public static class IndexServiceCollectionExtensions
{
    public static IServiceCollection AddIndexProvider<TIndexProvider>(this IServiceCollection services)
        where TIndexProvider : class, IIndexProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IIndexProvider, TIndexProvider>());

        return services;
    }

    public static IServiceCollection AddScopedIndexProvider<TScopedIndexProvider>(this IServiceCollection services)
        where TScopedIndexProvider : class, IScopedIndexProvider
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IScopedIndexProvider, TScopedIndexProvider>());

        return services;
    }
}
