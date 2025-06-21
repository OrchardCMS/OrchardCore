using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class DefaultIndexProfileManager : IIndexProfileManager
{
    private readonly IIndexProfileStore _store;
    private readonly IEnumerable<IIndexProfileHandler> _handlers;
    private readonly ILogger _logger;

    public DefaultIndexProfileManager(
        IIndexProfileStore store,
        IEnumerable<IIndexProfileHandler> handlers,
        ILogger<DefaultIndexProfileManager> logger)
    {
        _store = store;
        _handlers = handlers.Reverse();
        _logger = logger;
    }

    public async ValueTask<bool> DeleteAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        var deletingContext = new DeletingContext<IndexProfile>(indexProfile);
        await _handlers.InvokeAsync((handler, ctx) => handler.DeletingAsync(ctx), deletingContext, _logger).ConfigureAwait(false);

        if (string.IsNullOrEmpty(indexProfile.Id))
        {
            return false;
        }

        var removed = await _store.DeleteAsync(indexProfile).ConfigureAwait(false);

        var deletedContext = new DeletedContext<IndexProfile>(indexProfile);
        await _handlers.InvokeAsync((handler, ctx) => handler.DeletedAsync(ctx), deletedContext, _logger).ConfigureAwait(false);

        return removed;
    }

    public async ValueTask<IndexProfile> FindByIdAsync(string id)
    {
        var index = await _store.FindByIdAsync(id).ConfigureAwait(false);

        if (index is not null)
        {
            await LoadAsync(index).ConfigureAwait(false);

            return index;
        }

        return null;
    }

    public async ValueTask<IndexProfile> FindByNameAsync(string name)
    {
        var index = await _store.FindByNameAsync(name).ConfigureAwait(false);

        if (index is not null)
        {
            await LoadAsync(index).ConfigureAwait(false);

            return index;
        }

        return null;
    }


    public async ValueTask<IndexProfile> FindByNameAndProviderAsync(string indexName, string providerName)
    {
        var index = await _store.FindByIndexNameAndProviderAsync(indexName, providerName).ConfigureAwait(false);

        if (index is not null)
        {
            await LoadAsync(index).ConfigureAwait(false);

            return index;
        }

        return null;
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetByProviderAsync(string providerName)
    {
        var indexes = await _store.GetByProviderAsync(providerName).ConfigureAwait(false);

        foreach (var index in indexes)
        {
            await LoadAsync(index).ConfigureAwait(false);
        }

        return indexes;
    }

    public async ValueTask<IndexProfile> NewAsync(string providerName, string type, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(providerName);
        ArgumentException.ThrowIfNullOrEmpty(type);

        var id = IdGenerator.GenerateId();

        var index = new IndexProfile()
        {
            Id = id,
            ProviderName = providerName,
            Type = type,
        };

        var initializingContext = new InitializingContext<IndexProfile>(index, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger).ConfigureAwait(false);

        var initializedContext = new InitializedContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, _logger).ConfigureAwait(false);

        if (string.IsNullOrEmpty(index.Id))
        {
            index.Id = id;
        }

        return index;
    }

    public async ValueTask<PageResult<IndexProfile>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext
    {
        var result = await _store.PageAsync(page, pageSize, context).ConfigureAwait(false);

        foreach (var model in result.Models)
        {
            await LoadAsync(model).ConfigureAwait(false);
        }
        return result;
    }

    public async ValueTask CreateAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var creatingContext = new CreatingContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), creatingContext, _logger).ConfigureAwait(false);

        await _store.CreateAsync(index).ConfigureAwait(false);
        await _store.SaveChangesAsync().ConfigureAwait(false);

        var createdContext = new CreatedContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), createdContext, _logger).ConfigureAwait(false);
    }

    public async ValueTask UpdateAsync(IndexProfile index, JsonNode data = null)
    {
        ArgumentNullException.ThrowIfNull(index);

        var updatingContext = new UpdatingContext<IndexProfile>(index, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatingContext, _logger).ConfigureAwait(false);

        await _store.UpdateAsync(index).ConfigureAwait(false);
        await _store.SaveChangesAsync().ConfigureAwait(false);

        var updatedContext = new UpdatedContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger).ConfigureAwait(false);
    }

    public async ValueTask<ValidationResultDetails> ValidateAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var validatingContext = new ValidatingContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger).ConfigureAwait(false);

        var validatedContext = new ValidatedContext<IndexProfile>(index, validatingContext.Result);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatedAsync(ctx), validatedContext, _logger).ConfigureAwait(false);

        return validatingContext.Result;
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAllAsync()
    {
        var indexes = await _store.GetAllAsync().ConfigureAwait(false);

        foreach (var index in indexes)
        {
            await LoadAsync(index).ConfigureAwait(false);
        }

        return indexes;
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAsync(string providerName, string type)
    {
        var models = await _store.GetAsync(providerName, type).ConfigureAwait(false);

        foreach (var model in models)
        {
            await LoadAsync(model).ConfigureAwait(false);
        }

        return models;
    }

    public async ValueTask SynchronizeAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var synchronizedContext = new IndexProfileSynchronizedContext(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.SynchronizedAsync(ctx), synchronizedContext, _logger).ConfigureAwait(false);
    }

    public async ValueTask ResetAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var validatingContext = new IndexProfileResetContext(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.ResetAsync(ctx), validatingContext, _logger).ConfigureAwait(false);
    }

    private async ValueTask LoadAsync(IndexProfile index)
    {
        var loadedContext = new LoadedContext<IndexProfile>(index);

        await _handlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger).ConfigureAwait(false);
    }
}
