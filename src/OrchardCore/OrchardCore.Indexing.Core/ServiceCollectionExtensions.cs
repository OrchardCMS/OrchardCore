using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing.Core.Handlers;
using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexingCore(this IServiceCollection services)
    {
        services.AddScoped<IIndexEntityManager, DefaultIndexEntityManager>();
        services.AddScoped<IIndexEntityStore, DefaultIndexEntityStore>();
        services.AddScoped<IModelHandler<IndexEntity>, IndexEntityHandler>();

        return services;
    }
}
