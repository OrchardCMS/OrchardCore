using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Queries.Indexes;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Queries.Core.Services;

public sealed class QueryManager : IQueryManager
{
    private readonly ISession _session;
    private readonly QueryManagerSession _queryManagerSession;
    private readonly IServiceProvider _serviceProvider;

    public QueryManager(
        ISession session,
        QueryManagerSession queryManagerSession,
        IServiceProvider serviceProvider)
    {
        _session = session;
        _queryManagerSession = queryManagerSession;
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

    public Task<Query> GetQueryAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return _session.Query<Query, QueryIndex>(q => q.Name == name).FirstOrDefaultAsync();
    }

    public Task<IEnumerable<Query>> ListQueriesAsync(QueryContext context = null)
    {
        var query = GetQuery(context);

        return query.ListAsync();
    }

    public async Task<QueryResult> PageQueriesAsync(int page, int pageSize, QueryContext context = null)
    {
        var query = GetQuery(context);

        var skip = (page - 1) * pageSize;

        var result = new QueryResult
        {
            Count = await query.CountAsync(),
            Records = await query.Skip(skip).Take(pageSize).ListAsync()
        };

        return result;
    }

    public async Task SaveQueryAsync(params Query[] queries)
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
}
