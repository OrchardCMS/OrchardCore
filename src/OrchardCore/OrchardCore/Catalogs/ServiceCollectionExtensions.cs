using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Catalogs;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogManagers(this IServiceCollection services)
    {
        services.TryAddScoped(typeof(ICatalogManager<>), typeof(CatalogManager<>));
        services.TryAddScoped(typeof(INamedCatalogManager<>), typeof(NamedCatalogManager<>));
        services.TryAddScoped(typeof(ISourceCatalogManager<>), typeof(SourceCatalogManager<>));
        services.TryAddScoped(typeof(INamedSourceCatalogManager<>), typeof(NamedSourceCatalogManager<>));

        return services;
    }

    public static IServiceCollection AddCatalogs(this IServiceCollection services)
    {
        services.TryAddScoped(typeof(ICatalog<>), typeof(Catalog<>));
        services.TryAddScoped(typeof(INamedCatalog<>), typeof(NamedCatalog<>));
        services.TryAddScoped(typeof(ISourceCatalog<>), typeof(SourceCatalog<>));
        services.TryAddScoped(typeof(INamedSourceCatalog<>), typeof(NamedSourceCatalog<>));

        return services;
    }

    public static IServiceCollection AddCatalog<TModel>(this IServiceCollection services)
        where TModel : CatalogItem
    {
        services.AddScoped<ICatalog<TModel>, Catalog<TModel>>();

        return services;
    }

    public static IServiceCollection AddNamedCatalog<TModel>(this IServiceCollection services)
        where TModel : CatalogItem, INameAwareModel
    {
        services.AddScoped<ICatalog<TModel>, NamedCatalog<TModel>>();

        return services;
    }

    public static IServiceCollection AddSourceCatalog<TModel>(this IServiceCollection services)
        where TModel : CatalogItem, ISourceAwareModel
    {
        services.AddScoped<ISourceCatalog<TModel>, SourceCatalog<TModel>>();

        return services;
    }

    public static IServiceCollection AddNamedSourceCatalog<TModel>(this IServiceCollection services)
        where TModel : CatalogItem, INameAwareModel, ISourceAwareModel
    {
        services.AddScoped<INamedSourceCatalog<TModel>, NamedSourceCatalog<TModel>>();

        return services;
    }
}
