using Microsoft.Extensions.Logging;

namespace OrchardCore.Catalogs;

public class NamedSourceCatalogManager<T> : SourceCatalogManager<T>, INamedCatalogManager<T>, ISourceCatalogManager<T>, INamedSourceCatalogManager<T>
    where T : CatalogItem, INameAwareModel, ISourceAwareModel, new()
{
    protected readonly INamedSourceCatalog<T> NamedSourceModelStore;

    public NamedSourceCatalogManager(
        INamedSourceCatalog<T> store,
        IEnumerable<ICatalogEntryHandler<T>> handlers,
        ILogger<NamedSourceCatalogManager<T>> logger)
        : base(store, handlers, logger)
    {
        NamedSourceModelStore = store;
    }

    public async ValueTask<T> FindByNameAsync(string name)
    {
        var entry = await NamedSourceModelStore.FindByNameAsync(name);

        if (entry is not null)
        {
            await LoadAsync(entry);
        }

        return entry;
    }

    public async ValueTask<T> GetAsync(string name, string source)
    {
        var entry = await NamedSourceModelStore.GetAsync(name, source);

        if (entry is not null)
        {
            await LoadAsync(entry);
        }

        return entry;
    }
}
