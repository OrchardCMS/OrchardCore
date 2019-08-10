using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.Queries.Services
{
    public class QueryManager : IQueryManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISignal _signal;
        private readonly ISession _session;
        private IEnumerable<IQuerySource> _querySources;

        private const string QueriesDocumentCacheKey = nameof(QueriesDocumentCacheKey);

        public IChangeToken ChangeToken => _signal.GetToken(QueriesDocumentCacheKey);

        public QueryManager(
            IMemoryCache memoryCache,
            ISignal signal,
            ISession session,
            IEnumerable<IQuerySource> querySources)
        {
            _memoryCache = memoryCache;
            _signal = signal;
            _session = session;
            _querySources = querySources;
        }

        public async Task DeleteQueryAsync(string name)
        {
            var existing = await GetDocumentAsync();

            existing.Queries = existing.Queries
                .Remove(name)
                .ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);

            _session.Save(existing);
            _signal.DeferredSignalToken(QueriesDocumentCacheKey);

            return;
        }

        public async Task<Query> GetQueryAsync(string name)
        {
            var document = await GetDocumentAsync();

            if (document.Queries.TryGetValue(name, out var query))
            {
                return query;
            }

            return null;
        }

        public async Task<IEnumerable<Query>> ListQueriesAsync()
        {
            return (await GetDocumentAsync()).Queries.Values.ToList();
        }

        public async Task SaveQueryAsync(string name, Query query)
        {
            var existing = await GetDocumentAsync();
            existing.Queries = existing.Queries.Remove(name).SetItem(query.Name, query);

            _session.Save(existing);
            _signal.DeferredSignalToken(QueriesDocumentCacheKey);

            return;
        }

        private async Task<QueriesDocument> GetDocumentAsync()
        {
            QueriesDocument queries;

            if (!_memoryCache.TryGetValue(QueriesDocumentCacheKey, out queries))
            {
                var changeToken = ChangeToken;
                queries = await _session.Query<QueriesDocument>().FirstOrDefaultAsync();

                if (queries == null)
                {
                    queries = new QueriesDocument();

                    _session.Save(queries);
                    _signal.DeferredSignalToken(QueriesDocumentCacheKey);
                }
                else
                {
                    queries.Queries = queries.Queries.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase);
                    _memoryCache.Set(QueriesDocumentCacheKey, queries, changeToken);
                }
            }

            return queries;
        }

        public Task<object> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var querySource = _querySources.FirstOrDefault(q => q.Name == query.Source);

            if (querySource == null)
            {
                throw new ArgumentException("Query source not found: " + query.Source);
            }

            return querySource.ExecuteQueryAsync(query, parameters);
        }
    }
}
