using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Queries.Indexes;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Queries.Core.Services;

public sealed class DefaultQueryManager : IQueryManager
{
    private readonly ISession _session;
    private readonly DefaultQueryManagerSession _queryManagerSession;
    private readonly IEnumerable<IQueryHandler> _queryHandlers;
    private readonly ILogger<DefaultQueryManager> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DefaultQueryManager(
        ISession session,
        DefaultQueryManagerSession queryManagerSession,
        IEnumerable<IQueryHandler> queryHandlers,
        ILogger<DefaultQueryManager> logger,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _queryManagerSession = queryManagerSession;
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

        var queries = await _session.Query<Query, QueryIndex>(x => x.Name.IsIn(names)).ListAsync();

        foreach (var query in queries)
        {
            _session.Delete(query);
        }

        if (queries.Any())
        {
            await _session.SaveChangesAsync();
            await _queryManagerSession.GenerateKeyAsync();

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

    public Task<string> GetIdentifierAsync()
        => _queryManagerSession.GetKeyAsync();

    public async Task<Query> GetQueryAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var query = await _session.Query<Query, QueryIndex>(q => q.Name == name).FirstOrDefaultAsync();

        if (query != null)
        {
            await LoadAsync(query);
        }

        return query;
    }

    public async Task<IEnumerable<Query>> ListQueriesAsync(QueryContext context = null)
    {
        var records = await GetQuery(context).ListAsync();

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
        var query = GetQuery(context);

        var skip = (page - 1) * pageSize;

        var result = new ListQueryResult
        {
            Count = await query.CountAsync(),
            Records = await query.Skip(skip).Take(pageSize).ListAsync()
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

        // Update he base properties first before calling handler, to ensure we update query source when applicable.
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

        await _session.SaveAsync(query);
        await _session.SaveChangesAsync();
        await _queryManagerSession.GenerateKeyAsync();

        var updatedContext = new UpdatedQueryContext(query);
        await _queryHandlers.InvokeAsync((handler, context) => handler.UpdatedAsync(context), updatedContext, _logger);
    }

    public async Task SaveAsync(params Query[] queries)
    {
        if (queries?.Length == 0)
        {
            return;
        }

        foreach (var query in queries)
        {
            await _session.SaveAsync(query);
        }

        await _session.SaveChangesAsync();
        await _queryManagerSession.GenerateKeyAsync();
    }

    private IQuery<Query, QueryIndex> GetQuery(QueryContext context)
    {
        var query = _session.Query<Query, QueryIndex>();

        if (context == null)
        {
            return query;
        }

        if (!string.IsNullOrEmpty(context.Source))
        {
            query = query.Where(x => x.Source == context.Source);
        }

        if (!string.IsNullOrEmpty(context.Name))
        {
            query = query.Where(x => x.Name.Contains(context.Name));
        }

        if (context.Sorted)
        {
            query = query.OrderBy(x => x.Name);
        }

        return query;
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
