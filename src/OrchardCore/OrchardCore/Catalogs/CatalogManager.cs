using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Catalogs.Models;
using OrchardCore.Modules;

namespace OrchardCore.Catalogs;

public class CatalogManager<T> : ICatalogManager<T>
    where T : CatalogItem, new()
{
    protected readonly ICatalog<T> Catalog;
    protected readonly ILogger Logger;
    protected readonly IEnumerable<ICatalogEntryHandler<T>> Handlers;

    public CatalogManager(
        ICatalog<T> catalog,
        IEnumerable<ICatalogEntryHandler<T>> handlers,
        ILogger<CatalogManager<T>> logger)
    {
        Catalog = catalog;
        Handlers = handlers;
        Logger = logger;
    }

    protected CatalogManager(
        ICatalog<T> store,
        IEnumerable<ICatalogEntryHandler<T>> handlers,
        ILogger logger)
    {
        Catalog = store;
        Handlers = handlers.Reverse();
        Logger = logger;
    }

    public async ValueTask<bool> DeleteAsync(T entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var deletingContext = new DeletingContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.DeletingAsync(ctx), deletingContext, Logger);

        if (string.IsNullOrEmpty(entry.ItemId))
        {
            return false;
        }

        var removed = await Catalog.DeleteAsync(entry);

        await DeletedAsync(entry);

        var deletedContext = new DeletedContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.DeletedAsync(ctx), deletedContext, Logger);

        return removed;
    }

    public async ValueTask<T> FindByIdAsync(string id)
    {
        var entry = await Catalog.FindByIdAsync(id);

        if (entry is not null)
        {
            await LoadAsync(entry);

            return entry;
        }

        return null;
    }

    public virtual async ValueTask<T> NewAsync(JsonNode data = null)
    {
        var id = IdGenerator.GenerateId();

        var entry = new T()
        {
            ItemId = id,
        };

        var initializingContext = new InitializingContext<T>(entry, data);
        await Handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, Logger);

        var initializedContext = new InitializedContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, Logger);

        if (string.IsNullOrEmpty(entry.ItemId))
        {
            entry.ItemId = id;
        }

        return entry;
    }

    public async ValueTask<PageResult<T>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext
    {
        var result = await Catalog.PageAsync(page, pageSize, context);

        foreach (var entry in result.Entries)
        {
            await LoadAsync(entry);
        }

        return result;
    }

    public async ValueTask CreateAsync(T entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var creatingContext = new CreatingContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), creatingContext, Logger);

        await Catalog.CreateAsync(entry);
        await Catalog.SaveChangesAsync();

        var createdContext = new CreatedContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), createdContext, Logger);
    }

    public async ValueTask UpdateAsync(T entry, JsonNode data = null)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var updatingContext = new UpdatingContext<T>(entry, data);
        await Handlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatingContext, Logger);

        await Catalog.UpdateAsync(entry);
        await Catalog.SaveChangesAsync();

        var updatedContext = new UpdatedContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, Logger);
    }

    public async ValueTask<ValidationResultDetails> ValidateAsync(T entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var validatingContext = new ValidatingContext<T>(entry);
        await Handlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, Logger);

        var validatedContext = new ValidatedContext<T>(entry, validatingContext.Result);
        await Handlers.InvokeAsync((handler, ctx) => handler.ValidatedAsync(ctx), validatedContext, Logger);

        return validatingContext.Result;
    }

    public async ValueTask<IEnumerable<T>> GetAllAsync()
    {
        var models = await Catalog.GetAllAsync();

        foreach (var model in models)
        {
            await LoadAsync(model);
        }

        return models;
    }

    protected virtual ValueTask DeletedAsync(T entry)
    {
        return ValueTask.CompletedTask;
    }

    protected virtual async Task LoadAsync(T entry)
    {
        var loadedContext = new LoadedContext<T>(entry);

        await Handlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, Logger);
    }
}
