using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Indexing;
using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class DefaultIndexProfileManager : IIndexProfileManager
{
    private readonly IIndexProfileStore _store;
    private readonly IDistributedLock _distributedLock;
    private readonly IEnumerable<IIndexProfileHandler> _handlers;
    private readonly ILogger _logger;

    public DefaultIndexProfileManager(
        IIndexProfileStore store,
        IDistributedLock distributedLock,
        IEnumerable<IIndexProfileHandler> handlers,
        ILogger<DefaultIndexProfileManager> logger)
    {
        _store = store;
        _distributedLock = distributedLock;
        _handlers = handlers;
        _logger = logger;
    }

    public async ValueTask<bool> DeleteAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        var deletingContext = new DeletingContext<IndexProfile>(indexProfile);
        await _handlers.InvokeAsync((handler, ctx) => handler.DeletingAsync(ctx), deletingContext, _logger);

        if (string.IsNullOrEmpty(indexProfile.Id))
        {
            return false;
        }

        var removed = await _store.DeleteAsync(indexProfile);

        var deletedContext = new DeletedContext<IndexProfile>(indexProfile);
        await _handlers.InvokeAsync((handler, ctx) => handler.DeletedAsync(ctx), deletedContext, _logger);

        return removed;
    }

    public async ValueTask<IndexProfile> FindByIdAsync(string id)
    {
        var index = await _store.FindByIdAsync(id);

        if (index is not null)
        {
            await LoadAsync(index);

            return index;
        }

        return null;
    }

    public async ValueTask<IndexProfile> FindByNameAsync(string name)
    {
        var index = await _store.FindByNameAsync(name);

        if (index is not null)
        {
            await LoadAsync(index);

            return index;
        }

        return null;
    }

    public async ValueTask<IndexProfile> FindByNameAndProviderAsync(string indexName, string providerName)
    {
        var index = await _store.FindByIndexNameAndProviderAsync(indexName, providerName);

        if (index is not null)
        {
            await LoadAsync(index);

            return index;
        }

        return null;
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetByProviderAsync(string providerName)
    {
        var indexes = await _store.GetByProviderAsync(providerName);

        foreach (var index in indexes)
        {
            await LoadAsync(index);
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
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializingAsync(ctx), initializingContext, _logger);

        var initializedContext = new InitializedContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.InitializedAsync(ctx), initializedContext, _logger);

        if (string.IsNullOrEmpty(index.Id))
        {
            index.Id = id;
        }

        return index;
    }

    public async ValueTask<PageResult<IndexProfile>> PageAsync<TQuery>(int page, int pageSize, TQuery context)
        where TQuery : QueryContext
    {
        var result = await _store.PageAsync(page, pageSize, context);

        foreach (var model in result.Models)
        {
            await LoadAsync(model);
        }
        return result;
    }

    public async ValueTask CreateAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var creatingContext = new CreatingContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatingAsync(ctx), creatingContext, _logger);

        await _store.CreateAsync(index);
        await _store.SaveChangesAsync();

        var createdContext = new CreatedContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.CreatedAsync(ctx), createdContext, _logger);
    }

    public async ValueTask UpdateAsync(IndexProfile index, JsonNode data = null)
    {
        ArgumentNullException.ThrowIfNull(index);

        var updatingContext = new UpdatingContext<IndexProfile>(index, data);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatingAsync(ctx), updatingContext, _logger);

        await _store.UpdateAsync(index);
        await _store.SaveChangesAsync();

        var updatedContext = new UpdatedContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.UpdatedAsync(ctx), updatedContext, _logger);
    }

    public async ValueTask<ValidationResultDetails> ValidateAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var validatingContext = new ValidatingContext<IndexProfile>(index);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatingAsync(ctx), validatingContext, _logger);

        var validatedContext = new ValidatedContext<IndexProfile>(index, validatingContext.Result);
        await _handlers.InvokeAsync((handler, ctx) => handler.ValidatedAsync(ctx), validatedContext, _logger);

        return validatingContext.Result;
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAllAsync()
    {
        var indexes = await _store.GetAllAsync();

        foreach (var index in indexes)
        {
            await LoadAsync(index);
        }

        return indexes;
    }

    public async ValueTask<IEnumerable<IndexProfile>> GetAsync(string providerName, string type)
    {
        var models = await _store.GetAsync(providerName, type);

        foreach (var model in models)
        {
            await LoadAsync(model);
        }

        return models;
    }

    public async ValueTask<bool> SynchronizeAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        (var locker, var locked) = await _distributedLock.TryAcquireLockAsync($"IndexProfileSynchronize-{index.Id}", TimeSpan.FromSeconds(5));

        if (!locked)
        {
            _logger.LogWarning("Unable to acquire lock for synchronizing index profile '{IndexProfileId}'", index.Id);

            await locker.DisposeAsync();

            return false;
        }

        try
        {
            var synchronizedContext = new IndexProfileSynchronizedContext(index);

            await _handlers.InvokeAsync((handler, ctx) => handler.SynchronizedAsync(ctx), synchronizedContext, _logger);

            return true;
        }
        finally
        {
            await locker.DisposeAsync();
        }
    }

    public async ValueTask<bool> ResetAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        // Reset operation should be protected against concurrent executions.
        (var locker, var locked) = await _distributedLock.TryAcquireLockAsync($"IndexProfileReset-{index.Id}", TimeSpan.FromSeconds(5));

        if (!locked)
        {
            _logger.LogWarning("Unable to acquire lock for resetting index profile '{IndexProfileId}'", index.Id);

            await locker.DisposeAsync();

            return false;
        }

        try
        {
            var validatingContext = new IndexProfileResetContext(index);
            await _handlers.InvokeAsync((handler, ctx) => handler.ResetAsync(ctx), validatingContext, _logger);

            return true;
        }
        finally
        {
            await locker.DisposeAsync();
        }
    }

    private async ValueTask LoadAsync(IndexProfile index)
    {
        var loadedContext = new LoadedContext<IndexProfile>(index);

        await _handlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }
}
