using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Queries;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuerySource<TSource>(this IServiceCollection services, string sourceName)
        where TSource : class, IQuerySource
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        services.AddScoped<TSource>();
        services.AddScoped<IQuerySource>(sp => sp.GetService<TSource>());
        services.AddKeyedScoped<IQuerySource>(sourceName, (sp, key) => sp.GetService<TSource>());

        return services;
    }
}
