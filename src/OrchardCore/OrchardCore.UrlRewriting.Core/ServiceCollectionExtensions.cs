using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.UrlRewriting;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRewriteRuleSource<TSource>(this IServiceCollection services, string sourceName)
        where TSource : class, IUrlRewriteRuleSource
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceName);

        services.AddSingleton<TSource>();
        services.AddSingleton<IUrlRewriteRuleSource>(sp => sp.GetService<TSource>());
        services.AddKeyedSingleton<IUrlRewriteRuleSource>(sourceName, (sp, key) => sp.GetService<TSource>());

        return services;
    }
}
