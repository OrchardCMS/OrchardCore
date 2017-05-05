using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Orchard.Environment.Cache;
using YesSql;

namespace Orchard.Queries.Services
{
    public class QueryManager : IQueryManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ISession _session;
        private readonly ISignal _signal;

        private const string QueriesDocumentCacheKey = nameof(QueriesDocumentCacheKey);

        public QueryManager(
            ISignal signal,
            ISession session,
            IMemoryCache memoryCache)
        {
            _session = session;
            _memoryCache = memoryCache;
            _signal = signal;
        }

        public async Task DeleteQueryAsync(string name)
        {
            // Load the currently saved object otherwise it would create a new document
            // as the new session is not tracking the cached object.
            // TODO: Solve by having an Update method in Session

            var existing = await _session.QueryAsync<QueriesDocument>().FirstOrDefault();

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
            return (await GetDocumentAsync()).Queries[name];
        }

        public async Task<IEnumerable<Query>> ListQueriesAsync()
        {
            return (await GetDocumentAsync()).Queries.Values.ToList();
        }

        public async Task SaveQueryAsync(Query query)
        {
            // Load the currently saved object otherwise it would create a new document
            // as the new session is not tracking the cached object.
            // TODO: Solve by having an Update method in Session

            var existing = await _session.QueryAsync<QueriesDocument>().FirstOrDefault();

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
                queries = await _session.QueryAsync<QueriesDocument>().FirstOrDefault();

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
    }
}
