using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Modules;
using OrchardCore.Queries.Core.Models;

namespace OrchardCore.Queries.Core.Services;

public sealed class DefaultQueryManager : IQueryManager
{
    private readonly IDocumentManager<QueriesDocument> _documentManager;
    private readonly IEnumerable<IQueryHandler> _queryHandlers;
    private readonly ILogger<DefaultQueryManager> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DefaultQueryManager(
        IDocumentManager<QueriesDocument> documentManager,
        IEnumerable<IQueryHandler> queryHandlers,
        ILogger<DefaultQueryManager> logger,
        IServiceProvider serviceProvider)
    {
        _documentManager = documentManager;
        _queryHandlers = queryHandlers;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> DeleteQueryAsync(params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            return false;
        }

        var document = await _documentManager.GetOrCreateMutableAsync();

        var queries = new List<Query>();

        foreach (var name in names)
        {
            if (!document.Queries.TryGetValue(name, out var query))
            {
                continue;
            }

            var deletingContext = new DeletingQueryContext(query);
            await _queryHandlers.InvokeAsync((handler, context) => handler.DeletingAsync(context), deletingContext, _logger);

            queries.Add(query);
            document.Queries.Remove(name);
        }

        if (queries.Count > 0)
        {
            await _documentManager.UpdateAsync(document);

            foreach (var query in queries)
            {
                var deletedContext = new DeletedQueryContext(query);
                await _queryHandlers.InvokeAsync((handler, context) => handler.DeletedAsync(context), deletedContext, _logger);
            }

            return true;
        }

        return false;
    }

    public Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
    {
        ArgumentNullException.ThrowIfNull(query);

        var source = _serviceProvider.GetRequiredKeyedService<IQuerySource>(query.Source);

        return source.ExecuteQueryAsync(query, parameters);
    }

    public async Task<string> GetIdentifierAsync()
        => (await _documentManager.GetOrCreateImmutableAsync()).Identifier;

    public async Task<Query> GetQueryAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var document = await _documentManager.GetOrCreateImmutableAsync();

        if (document.Queries.TryGetValue(name, out var query))
        {
            await LoadAsync(query);
        }

        return query;
    }

    public async Task<IEnumerable<Query>> ListQueriesAsync(QueryContext context = null)
    {
        var records = await LocateQueriesAsync(context);

        foreach (var record in records)
        {
            await LoadAsync(record);
        }

        return records;
    }

    public async Task<Query> NewAsync(string source, JsonNode data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var querySource = _serviceProvider.GetKeyedService<IQuerySource>(source);

        if (querySource == null)
        {
            _logger.LogWarning("Unable to find a query-source that can handle the source '{Source}'.", source);

            return null;
        }

        var query = new Query()
        {
            Source = source,
        };

        var initializingContext = new InitializingQueryContext(query, data);

        await _queryHandlers.InvokeAsync((handler, context) => handler.InitializingAsync(context), initializingContext, _logger);

        if (data != null)
        {
            // During creating a query, we attempt to set the query name.
            query.Name = data[nameof(Query.Name)]?.GetValue<string>();

            UpdateBaseProperties(data, query);
        }

        var initializedContext = new InitializedQueryContext(query);

        await _queryHandlers.InvokeAsync((handler, context) => handler.InitializedAsync(context), initializedContext, _logger);

        // Set the source again after calling handlers to prevent handlers from updating the source during initialization.
        query.Source = source;

        return query;
    }

    public async Task<ListQueryResult> PageQueriesAsync(int page, int pageSize, QueryContext context = null)
    {
        var records = await LocateQueriesAsync(context);

        var skip = (page - 1) * pageSize;

        var result = new ListQueryResult
        {
            Count = records.Count(),
            Records = records.Skip(skip).Take(pageSize).ToArray()
        };

        foreach (var record in result.Records)
        {
            await LoadAsync(record);
        }

        return result;
    }

    public async Task UpdateAsync(Query query, JsonNode data = null)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Update the base properties first before calling the handler, to ensure we update the query source when applicable.
        if (data != null)
        {
            UpdateBaseProperties(data, query);

            // During the update, we allow changing the source.
            var sourceName = data[nameof(Query.Source)]?.GetValue<string>();

            if (!string.IsNullOrEmpty(sourceName) && query.Source != sourceName)
            {
                var querySource = _serviceProvider.GetKeyedService<IQuerySource>(sourceName);

                if (querySource != null)
                {
                    query.Source = querySource.Name;
                }
            }
        }

        var updatingContext = new UpdatingQueryContext(query, data);
        await _queryHandlers.InvokeAsync((handler, context) => handler.UpdatingAsync(context), updatingContext, _logger);

        var document = await _documentManager.GetOrCreateMutableAsync();
        document.Queries[query.Name] = query;
        await _documentManager.UpdateAsync(document);

        var updatedContext = new UpdatedQueryContext(query);
        await _queryHandlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), updatedContext, _logger);
    }

    public async Task SaveAsync(params Query[] queries)
    {
        if (queries == null || queries.Length == 0)
        {
            return;
        }

        var document = await _documentManager.GetOrCreateMutableAsync();

        foreach (var query in queries)
        {
            document.Queries[query.Name] = query;
        }

        await _documentManager.UpdateAsync(document);
    }

    private async Task<IEnumerable<Query>> LocateQueriesAsync(QueryContext context)
    {
        var document = await _documentManager.GetOrCreateImmutableAsync();

        if (context == null)
        {
            return document.Queries.Values;
        }

        var queries = document.Queries.Values.AsEnumerable();

        if (!string.IsNullOrEmpty(context.Source))
        {
            queries = queries.Where(x => x.Source.Equals(context.Source, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(context.Name))
        {
            queries = queries.Where(x => x.Name.Contains(context.Name, StringComparison.OrdinalIgnoreCase));
        }

        if (context.Sorted)
        {
            queries = queries.OrderBy(x => x.Name);
        }

        return queries;
    }

    private Task LoadAsync(Query query)
    {
        var loadedContext = new LoadedQueryContext(query);

        return _queryHandlers.InvokeAsync((handler, context) => handler.LoadedAsync(context), loadedContext, _logger);
    }

    private static void UpdateBaseProperties(JsonNode data, Query query)
    {
        query.Schema = data[nameof(Query.Schema)]?.GetValue<string>();

        // For backward compatibility, we use the key 'ReturnDocuments'.
        var returnDocuments = data["ReturnDocuments"];

        if (returnDocuments != null)
        {
            query.ReturnContentItems = returnDocuments.GetValue<bool>();
        }

        var returnContentItems = data[nameof(Query.ReturnContentItems)];

        if (returnContentItems != null)
        {
            query.ReturnContentItems = returnContentItems.GetValue<bool>();
        }
    }
}
