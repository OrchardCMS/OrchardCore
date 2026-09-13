using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Abstractions.Indexing;
using OrchardCore.BackgroundJobs;
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
        _handlers = handlers;
        _logger = logger;
    }

    public async ValueTask<bool> DeleteAsync(IndexProfile indexProfile)
    {
        ArgumentNullException.ThrowIfNull(indexProfile);

        var deletingContext = new DeletingContext<IndexProfile>(indexProfile);
        await InvokeRequiredAsync(deletingContext, static (handler, ctx) => handler.DeletingAsync(ctx));

        if (string.IsNullOrEmpty(indexProfile.Id))
        {
            return false;
        }

        var removed = await _store.DeleteAsync(indexProfile);

        var deletedContext = new DeletedContext<IndexProfile>(indexProfile);
        await InvokeRequiredAsync(deletedContext, static (handler, ctx) => handler.DeletedAsync(ctx));

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
        await InvokeRequiredAsync(initializingContext, static (handler, ctx) => handler.InitializingAsync(ctx));

        var initializedContext = new InitializedContext<IndexProfile>(index);
        await InvokeRequiredAsync(initializedContext, static (handler, ctx) => handler.InitializedAsync(ctx));

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
        await InvokeRequiredAsync(creatingContext, static (handler, ctx) => handler.CreatingAsync(ctx));

        await _store.CreateAsync(index);

        var createdContext = new CreatedContext<IndexProfile>(index);
        await InvokeRequiredAsync(createdContext, static (handler, ctx) => handler.CreatedAsync(ctx));
    }

    public async ValueTask UpdateAsync(IndexProfile index, JsonNode data = null)
    {
        ArgumentNullException.ThrowIfNull(index);

        // Updating handlers apply incoming recipe/API data to the tracked instance.
        // Keep a deep snapshot so a rejected update cannot leak into a later save.
        var original = new IndexProfile
        {
            Id = index.Id, Name = index.Name, ProviderName = index.ProviderName, Type = index.Type,
            IndexName = index.IndexName, IndexFullName = index.IndexFullName,
            CreatedUtc = index.CreatedUtc, Author = index.Author, OwnerId = index.OwnerId,
            Properties = index.Properties?.DeepClone().AsObject(),
        };
        try
        {
            var updatingContext = new UpdatingContext<IndexProfile>(index, data);
            await InvokeRequiredAsync(updatingContext, static (handler, ctx) => handler.UpdatingAsync(ctx));

            var validation = await ValidateAsync(index);
            if (!validation.Succeeded)
            {
                throw new IndexProfileValidationException(validation.Errors);
            }
        }
        catch
        {
            index.Id = original.Id;
            index.Name = original.Name;
            index.ProviderName = original.ProviderName;
            index.Type = original.Type;
            index.IndexName = original.IndexName;
            index.IndexFullName = original.IndexFullName;
            index.CreatedUtc = original.CreatedUtc;
            index.Author = original.Author;
            index.OwnerId = original.OwnerId;
            index.Properties = original.Properties;
            throw;
        }

        await _store.UpdateAsync(index);

        var updatedContext = new UpdatedContext<IndexProfile>(index);
        await InvokeRequiredAsync(updatedContext, static (handler, ctx) => handler.UpdatedAsync(ctx));
    }

    public async ValueTask<ValidationResultDetails> ValidateAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var validatingContext = new ValidatingContext<IndexProfile>(index);
        await InvokeRequiredAsync(validatingContext, static (handler, ctx) => handler.ValidatingAsync(ctx));

        var validatedContext = new ValidatedContext<IndexProfile>(index, validatingContext.Result);
        await InvokeRequiredAsync(validatedContext, static (handler, ctx) => handler.ValidatedAsync(ctx));

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

    public async ValueTask SynchronizeAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("IndexProfileManager_Synchronize", async scope =>
        {
            // Resolve services from the new scope to avoid using disposed services.
            var handlers = scope.ServiceProvider.GetServices<IIndexProfileHandler>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<DefaultIndexProfileManager>>();

            try
            {
                var synchronizedContext = new IndexProfileSynchronizedContext(index);
                await handlers.InvokeAsync((handler, ctx) => handler.SynchronizedAsync(ctx), synchronizedContext, logger);
            }
            catch (Exception ex)
            {
                // Log synchronization errors without failing the entire background job
                logger.LogError(ex, "Error synchronizing index profile {IndexName}. The synchronization will be retried on the next background task run.", index.Name);
            }
        });
    }

    public async ValueTask ResetAsync(IndexProfile index)
    {
        ArgumentNullException.ThrowIfNull(index);

        var validatingContext = new IndexProfileResetContext(index);
        await InvokeRequiredAsync(validatingContext, (handler, ctx) => handler.ResetAsync(ctx));
    }

    private async ValueTask LoadAsync(IndexProfile index)
    {
        var loadedContext = new LoadedContext<IndexProfile>(index);

        await _handlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }
    private async Task InvokeRequiredAsync<TContext>(TContext context, Func<IIndexProfileHandler, TContext, Task> invoke)
    {
        // Mutation and validation handlers are prerequisites, not best-effort notifications.
        // Post-persistence errors also propagate, but cannot imply rollback of committed work.
        foreach (var handler in _handlers)
        {
            await invoke(handler, context);
        }
    }

}
