using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Catalogs.Models;
using OrchardCore.Modules;

namespace OrchardCore.Catalogs;

public class SourceCatalogManager<T> : CatalogManager<T>, ISourceCatalogManager<T>
    where T : CatalogItem, ISourceAwareModel, new()
{
    protected readonly ISourceCatalog<T> SourceCatalog;

    public SourceCatalogManager(
        ISourceCatalog<T> sourceCatalog,
        IEnumerable<ICatalogEntryHandler<T>> handlers,
        ILogger<SourceCatalogManager<T>> logger)
        : base(sourceCatalog, handlers, logger)
    {
        SourceCatalog = sourceCatalog;
    }

    public async ValueTask<IReadOnlyCollection<T>> FindBySourceAsync(string source)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var entries = (await Catalog.GetAllAsync()).Where(x => x.Source == source).ToArray();

        foreach (var entry in entries)
        {
            await LoadAsync(entry);
        }

        return entries;
    }

    public async ValueTask<IReadOnlyCollection<T>> GetAsync(string source)
    {
        var entries = await SourceCatalog.GetAsync(source);

        foreach (var entry in entries)
        {
            await LoadAsync(entry);
        }

        return entries;
    }

    public async ValueTask<T> NewAsync(string source, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var id = IdGenerator.GenerateId();

        var entry = new T()
        {
            ItemId = id,
            Source = source,
        };

        var initializingContext = new InitializingContext<T>(entry, data);
        await Handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, Logger);

        var initializedContext = new InitializedContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, Logger);

        if (string.IsNullOrEmpty(entry.ItemId))
        {
            entry.ItemId = id;
        }

        entry.Source = source;

        return entry;
    }

    public override ValueTask<T> NewAsync(JsonNode data = null)
    {
        var source = data?["Source"]?.GetValue<string>();

        if (string.IsNullOrEmpty(source))
        {
            throw new InvalidOperationException("Data must contain a Source entry");
        }

        return NewAsync(source, data);
    }
}
