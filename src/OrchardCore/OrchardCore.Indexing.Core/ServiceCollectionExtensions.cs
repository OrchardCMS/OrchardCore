using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Indexing.Core.Handlers;

namespace OrchardCore.Indexing.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexingCore(this IServiceCollection services)
    {
        services.TryAddScoped<IIndexEntityManager, DefaultIndexEntityManager>();
        services.TryAddScoped<IIndexEntityStore, DefaultIndexEntityStore>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, IndexEntityHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IIndexEntityHandler, ContentIndexEntryHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IContentHandler, IndexingContentHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, IndexingAuthorizationHandler>());

        return services;
    }
}
