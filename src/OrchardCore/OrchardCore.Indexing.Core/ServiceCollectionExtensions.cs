using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexingCore(this IServiceCollection services)
    {
        services.TryAddScoped<IIndexProfileManager, DefaultIndexProfileManager>();
        services.TryAddScoped<IIndexProfileStore, DefaultIndexProfileStore>();
        services.AddIndexProfileHandler<DefaultIndexProfileHandler>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, IndexingAuthorizationHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IModularTenantEvents, IndexInitializerService>());

        return services;
    }

    /// <summary>
    /// Provides a way to register an indexing source with a specific manager, document manager, naming provider, and make available on startup.
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <typeparam name="TDocumentManager"></typeparam>
    /// <typeparam name="TNamingProvider"></typeparam>
    /// <param name="services"></param>
    /// <param name="providerName"></param>
    /// <param name="implementationType"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IServiceCollection AddIndexingSource<TManager, TDocumentManager, TNamingProvider>(this IServiceCollection services, string providerName, string implementationType, Action<IndexingOptionsEntry> action = null)
        where TManager : class, IIndexManager
        where TDocumentManager : class, IDocumentIndexManager
        where TNamingProvider : class, IIndexNameProvider
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSourceServices<TManager, TDocumentManager, TNamingProvider>(providerName);

        services.Configure<IndexingOptions>(options =>
        {
            options.AddIndexingSource(providerName, implementationType, action);
        });

        return services;
    }

    /// <summary>
    /// Provides a way to register an indexing source with a specific manager, document manager,
    /// and naming provider when the provider configuration exists.
    /// </summary>
    /// <typeparam name="TManager"></typeparam>
    /// <typeparam name="TDocumentManager"></typeparam>
    /// <typeparam name="TNamingProvider"></typeparam>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="services"></param>
    /// <param name="providerName"></param>
    /// <param name="implementationType"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IServiceCollection AddIndexingSource<TManager, TDocumentManager, TNamingProvider, TOptions>(this IServiceCollection services, string providerName, string implementationType, Action<IndexingOptionsEntry> action = null)
        where TManager : class, IIndexManager
        where TDocumentManager : class, IDocumentIndexManager
        where TNamingProvider : class, IIndexNameProvider
        where TOptions : class, ISearchProviderOptions
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.AddIndexingSourceServices<TManager, TDocumentManager, TNamingProvider>(providerName);

        services
            .AddOptions<IndexingOptions>()
            .Configure<IServiceProvider>((options, sp) =>
            {
                var o = sp.GetRequiredService<IOptions<TOptions>>();

                if (o.Value.ConfigurationExists())
                {
                    options.AddIndexingSource(providerName, implementationType, action);
                }
            });

        return services;
    }

    private static IServiceCollection AddIndexingSourceServices<TManager, TDocumentManager, TNamingProvider>(this IServiceCollection services, string providerName)
       where TManager : class, IIndexManager
       where TDocumentManager : class, IDocumentIndexManager
       where TNamingProvider : class, IIndexNameProvider
    {
        services.TryAddScoped<TManager>();
        services.TryAddKeyedScoped<IIndexManager>(providerName, (sp, key) => sp.GetRequiredService<TManager>());
        services.TryAddScoped<TDocumentManager>();
        services.TryAddKeyedScoped<IDocumentIndexManager>(providerName, (sp, key) => sp.GetRequiredService<TDocumentManager>());
        services.TryAddKeyedScoped<IIndexNameProvider, TNamingProvider>(providerName);

        return services;
    }

    public static IServiceCollection AddIndexProfileHandler<THandler>(this IServiceCollection services)
        where THandler : class, IIndexProfileHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexProfileHandler, THandler>());

        return services;
    }
}
