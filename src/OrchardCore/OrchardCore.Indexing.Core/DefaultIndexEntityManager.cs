using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class DefaultIndexEntityManager : IIndexEntityManager
{
    private readonly IIndexEntityStore _store;
    private readonly IEnumerable<IIndexEntityHandler> _handlers;
    private readonly ILogger _logger;

    public DefaultIndexEntityManager(
        IIndexEntityStore store,
        IEnumerable<IIndexEntityHandler> handlers,
        ILogger<DefaultIndexEntityManager> logger)
    {
        _store = store;
        _handlers = handlers.Reverse();
        _logger = logger;
    }

    public async ValueTask<bool> DeleteAsync(IndexEntity model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var deletingContext = new DeletingContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.DeletingAsync(ctx), deletingContext, _logger);

        if (string.IsNullOrEmpty(model.Id))
        {
            return false;
        }

        var removed = await _store.DeleteAsync(model);

        var deletedContext = new DeletedContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.DeletedAsync(ctx), deletedContext, _logger);

        return removed;
    }

    public async ValueTask<IndexEntity> FindByIdAsync(string id)
    {
        var model = await _store.FindByIdAsync(id);
        if (model is not null)
        {
            await LoadAsync(model);

            return model;
        }

        return null;
    }

    public async ValueTask<IndexEntity> FindByNameAndProviderAsync(string indexName, string providerName)
    {
        var model = await _store.FindByNameAndProviderAsync(indexName, providerName);

        if (model is not null)
        {
            await LoadAsync(model);

            return model;
        }

        return null;
    }

    public async ValueTask<IndexEntity> NewAsync(string providerName, string type, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        var id = IdGenerator.GenerateId();

        var model = new IndexEntity()
        {
            Id = id,
            ProviderName = providerName,
            Type = type,
        };

        var initializingContext = new InitializingContext<IndexEntity>(model, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        var initializedContext = new InitializedContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, _logger);

        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = id;
        }

        return model;
    }

    public async ValueTask<PageResult<IndexEntity>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext
    {
        var result = await _store.PageAsync(page, pageSize, context);

        foreach (var model in result.Models)
        {
            await LoadAsync(model);
        }
        return result;
    }

    public async ValueTask CreateAsync(IndexEntity model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var creatingContext = new CreatingContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), creatingContext, _logger);

        await _store.CreateAsync(model);
        await _store.SaveChangesAsync();

        var createdContext = new CreatedContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), createdContext, _logger);
    }

    public async ValueTask UpdateAsync(IndexEntity model, JsonNode data = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        var updatingContext = new UpdatingContext<IndexEntity>(model, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatingContext, _logger);

        await _store.UpdateAsync(model);
        await _store.SaveChangesAsync();

        var updatedContext = new UpdatedContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger);
    }

    public async ValueTask<ValidationResultDetails> ValidateAsync(IndexEntity model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var validatingContext = new ValidatingContext<IndexEntity>(model);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger);

        var validatedContext = new ValidatedContext<IndexEntity>(model, validatingContext.Result);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatedAsync(ctx), validatedContext, _logger);

        return validatingContext.Result;
    }

    public async ValueTask<IEnumerable<IndexEntity>> GetAllAsync()
    {
        var models = await _store.GetAllAsync();

        foreach (var model in models)
        {
            await LoadAsync(model);
        }

        return models;
    }

    public async ValueTask<IEnumerable<IndexEntity>> GetAsync(string providerName, string type)
    {
        var models = await _store.GetAsync(providerName, type);

        foreach (var model in models)
        {
            await LoadAsync(model);
        }

        return models;
    }

    public async Task SynchronizeAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var synchronizedContext = new IndexEntitySynchronizedContext(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.SynchronizedAsync(ctx), synchronizedContext, _logger);
    }

    public async Task ResetAsync(IndexEntity index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var validatingContext = new IndexEntityResetContext(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.ResetAsync(ctx), validatingContext, _logger);
    }

    private async Task LoadAsync(IndexEntity index)
    {
        var loadedContext = new LoadedContext<IndexEntity>(index);

        await _handlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }
}
