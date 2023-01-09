using Microsoft.Extensions.DependencyInjection;
using YesSql.Indexes;

namespace OrchardCore.Data;

public static class IndexesServiceCollectionExtensions
{
    public static IServiceCollection AddIndexProvider<TIndexProvider>(this IServiceCollection services)
        where TIndexProvider : class, IIndexProvider
    {
        return services.AddSingleton<IIndexProvider, TIndexProvider>();
    }

    public static IServiceCollection AddScopedIndexProvider<TIndexProvider>(this IServiceCollection services)
        where TIndexProvider : class, IScopedIndexProvider
    {
        return services.AddScoped<IScopedIndexProvider, TIndexProvider>();
    }
}
