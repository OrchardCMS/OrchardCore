using System;
using System.Collections.Generic;
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
        private readonly ISession _session;
        private readonly ISignal _signal;
        private IEnumerable<IQuerySource> _querySources;

        private const string QueriesDocumentCacheKey = nameof(QueriesDocumentCacheKey);

        public IChangeToken ChangeToken => _signal.GetToken(QueriesDocumentCacheKey);

        public QueryManager(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache,
            IEnumerable<IQuerySource> querySources)
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
            _querySources = querySources;
        }

        public async Task DeleteQueryAsync(string name)
        {
            var existing = await GetDocumentAsync();

            if (existing.Queries.ContainsKey(name))
            {
                existing.Queries.Remove(name);
            }

            await SaveAsync(existing);

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

            existing.Queries.Remove(name);
            existing.Queries[query.Name] = query;

            await SaveAsync(existing);

            return;
        }

        private async Task<QueriesDocument> GetDocumentAsync()
        {
            QueriesDocument queries;

            if (!_memoryCache.TryGetValue(QueriesDocumentCacheKey, out queries))
            {
                var changeToken = _signal.GetToken(QueriesDocumentCacheKey);
                queries = await _session.Query<QueriesDocument>().FirstOrDefaultAsync();

                if (queries == null)
                {
                    queries = new QueriesDocument();
                    await SaveAsync(queries);
                }
                else
                {
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

        private async Task SaveAsync(QueriesDocument document)
        {
            _session.Save(document);
            await _session.CommitAsync();
            _signal.SignalToken(QueriesDocumentCacheKey);
        }
    }
}
