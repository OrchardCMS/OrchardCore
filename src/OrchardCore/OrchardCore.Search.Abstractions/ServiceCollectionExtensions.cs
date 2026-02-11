using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Search.Abstractions;

namespace OrchardCore.Search;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSearchService<TService>(this IServiceCollection services, string providerName)
        where TService : class, ISearchService
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);

        services.TryAddScoped<TService>();
        services.AddScoped<ISearchService>(sp => sp.GetRequiredService<TService>());
        services.TryAddEnumerable(ServiceDescriptor.KeyedScoped<ISearchService, TService>(providerName, (sp, key) => sp.GetRequiredService<TService>()));

        return services;
    }
}
