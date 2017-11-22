using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
            // Ensure QueriesDocument exists 
            await GetDocumentAsync();

            // Load the currently saved object otherwise it would create a new document
            // as the new session is not tracking the cached object.
            // TODO: Solve by having an Import method in Session or an Id on the document

            var existing = await _session.Query<QueriesDocument>().FirstOrDefaultAsync();

            if (existing.Queries.ContainsKey(name))
            {
                existing.Queries.Remove(name);
            }

            _session.Save(existing);

            _memoryCache.Set(QueriesDocumentCacheKey, existing);
            _signal.SignalToken(QueriesDocumentCacheKey);

            return;
        }

        public async Task<Query> GetQueryAsync(string name)
        {
            var document = await GetDocumentAsync();

            if(document.Queries.TryGetValue(name, out var query))
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
            // Ensure QueriesDocument exists 
            await GetDocumentAsync();

            // Load the currently saved object otherwise it would create a new document
            // as the new session is not tracking the cached object.
            // TODO: Solve by having an Import method in Session

            var existing = await _session.Query<QueriesDocument>().FirstOrDefaultAsync();

            existing.Queries.Remove(name);
            existing.Queries[query.Name] = query;

            _session.Save(existing);

            _memoryCache.Set(QueriesDocumentCacheKey, existing);
            _signal.SignalToken(QueriesDocumentCacheKey);

            return;
        }

        private async Task<QueriesDocument> GetDocumentAsync()
        {
            QueriesDocument queries;

            if (!_memoryCache.TryGetValue(QueriesDocumentCacheKey, out queries))
            {
                queries = await _session.Query<QueriesDocument>().FirstOrDefaultAsync();

                if (queries == null)
                {
                    lock (_memoryCache)
                    {
                        if (!_memoryCache.TryGetValue(QueriesDocumentCacheKey, out queries))
                        {
                            queries = new QueriesDocument();

                            _session.Save(queries);
                            _memoryCache.Set(QueriesDocumentCacheKey, queries);
                            _signal.SignalToken(QueriesDocumentCacheKey);
                        }
                    }
                }
                else
                {
                    _memoryCache.Set(QueriesDocumentCacheKey, queries);
                    _signal.SignalToken(QueriesDocumentCacheKey);
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
