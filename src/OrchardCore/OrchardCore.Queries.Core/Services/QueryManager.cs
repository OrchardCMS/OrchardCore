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

    public async Task<IEnumerable<Query>> ListAllAsync(bool sorted = false)
    {
        var query = _session.Query<Query, QueryIndex>();

        if (sorted)
        {
            return await query.OrderBy(q => q.Name).ListAsync();
        }

        return await query.ListAsync();
    }

    public async Task<IEnumerable<Query>> ListBySourceAsync(string source, bool sorted = false)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        var query = _session.Query<Query, QueryIndex>(q => q.Source == source);

        if (sorted)
        {
            return await query.OrderBy(q => q.Name).ListAsync();
        }

        return await query.ListAsync();
    }

    public async Task SaveQueryAsync(params Query[] queries)
    {
        ArgumentNullException.ThrowIfNull(queries);

        if (queries.Length == 0)
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
}
