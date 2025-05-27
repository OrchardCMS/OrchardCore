using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexingCore(this IServiceCollection services)
    {
        services.TryAddScoped<IIndexEntityManager, DefaultIndexEntityManager>();
        services.TryAddScoped<IIndexEntityStore, DefaultIndexEntityStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, DefaultIndexEntityHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IContentHandler, IndexingContentHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, IndexingAuthorizationHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IModularTenantEvents, IndexInitializerService>());

        return services;
    }

    public static IServiceCollection AddIndexingSource<TManager, TDocumentManager, TNamingProvider>(this IServiceCollection services, string providerName, string implementationType, Action<IndexingOptionsEntry> action = null)
        where TManager : class, IIndexManager
        where TDocumentManager : class, IIndexDocumentManager
        where TNamingProvider : class, IIndexNameProvider
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(implementationType);

        services.TryAddScoped<TManager>();
        services.AddKeyedScoped<IIndexManager>(providerName, (sp, key) => sp.GetRequiredService<TManager>());
        services.TryAddScoped<TDocumentManager>();
        services.AddKeyedScoped<IIndexDocumentManager>(providerName, (sp, key) => sp.GetRequiredService<TDocumentManager>());
        services.AddKeyedScoped<IIndexNameProvider, TNamingProvider>(providerName);

        services.Configure<IndexingOptions>(options =>
        {
            options.AddIndexingSource(providerName, implementationType, action);
        });

        return services;
    }

    public static IServiceCollection AddIndexEntityHandler<THandler>(this IServiceCollection services)
        where THandler : class, IIndexEntityHandler
    {
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, THandler>());

        return services;
    }
}
