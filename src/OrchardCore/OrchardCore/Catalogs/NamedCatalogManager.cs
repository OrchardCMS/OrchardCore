using Microsoft.Extensions.Logging;

namespace OrchardCore.Catalogs;

public class NamedCatalogManager<T> : CatalogManager<T>, INamedCatalogManager<T>
    where T : CatalogItem, INameAwareModel, new()
{
    protected readonly INamedCatalog<T> NamedCatalog;

    public NamedCatalogManager(
        INamedCatalog<T> catalog,
        IEnumerable<ICatalogEntryHandler<T>> handlers,
        ILogger<NamedCatalogManager<T>> logger)
        : base(catalog, handlers, logger)
    {
        NamedCatalog = catalog;
    }

    protected NamedCatalogManager(
        INamedCatalog<T> catalog,
        IEnumerable<ICatalogEntryHandler<T>> handlers,
        ILogger logger)
        : base(catalog, handlers, logger)
    {
        NamedCatalog = catalog;
    }

    public async ValueTask<T> FindByNameAsync(string name)
    {
        var entry = await NamedCatalog.FindByNameAsync(name);

        if (entry is not null)
        {
            await LoadAsync(entry);
        }

        return entry;
    }
}
