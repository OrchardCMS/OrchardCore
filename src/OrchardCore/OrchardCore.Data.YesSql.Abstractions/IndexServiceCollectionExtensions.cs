using Microsoft.Extensions.DependencyInjection;
using YesSql.Indexes;

namespace OrchardCore.Data;

public static class IndexServiceCollectionExtensions
{
    public static IServiceCollection AddIndexProvider<TIndexProvider>(this IServiceCollection services)
        where TIndexProvider : class, IIndexProvider
    {
        return services.AddSingleton<IIndexProvider, TIndexProvider>();
    }

    public static IServiceCollection AddScopedIndexProvider<TScopedIndexProvider>(this IServiceCollection services)
        where TScopedIndexProvider : class, IScopedIndexProvider
    {
        return services.AddScoped<IScopedIndexProvider, TScopedIndexProvider>();
    }
}
