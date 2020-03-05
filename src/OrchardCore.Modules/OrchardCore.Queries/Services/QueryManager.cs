using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using YesSql;

namespace OrchardCore.Queries.Services
{
    public class QueryManager : IQueryManager
    {
        private const string QueriesDocumentCacheKey = nameof(QueriesDocumentCacheKey);

        private readonly ISignal _signal;
        private readonly ISession _session;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMemoryCache _memoryCache;
        private IEnumerable<IQuerySource> _querySources;

        public QueryManager(
            ISignal signal,
            ISession session,
            ISessionHelper sessionHelper,
            IMemoryCache memoryCache,
            IEnumerable<IQuerySource> querySources)
        {
            _signal = signal;
            _session = session;
            _sessionHelper = sessionHelper;
            _memoryCache = memoryCache;
            _querySources = querySources;
        }

        public IChangeToken ChangeToken => _signal.GetToken(QueriesDocumentCacheKey);

        public async Task DeleteQueryAsync(string name)
        {
            var existing = await LoadDocumentAsync();
            existing.Queries.Remove(name);

            _session.Save(existing);
            _signal.DeferredSignalToken(QueriesDocumentCacheKey);

            return;
        }

        public async Task<Query> LoadQueryAsync(string name)
        {
            var document = await LoadDocumentAsync();

            if (document.Queries.TryGetValue(name, out var query))
            {
                return query;
            }

            return null;
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
            if (query.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var existing = await LoadDocumentAsync();
            existing.Queries.Remove(name);
            existing.Queries[query.Name] = query;

            _session.Save(existing);
            _signal.DeferredSignalToken(QueriesDocumentCacheKey);

            return;
        }

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public Task<QueriesDocument> LoadDocumentAsync() => _sessionHelper.LoadForUpdateAsync<QueriesDocument>();

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        private async Task<QueriesDocument> GetDocumentAsync()
        {
            if (!_memoryCache.TryGetValue<QueriesDocument>(QueriesDocumentCacheKey, out var queries))
            {
                var changeToken = ChangeToken;

                queries = await _sessionHelper.GetForCachingAsync<QueriesDocument>();

                foreach (var query in queries.Queries.Values)
                {
                    query.IsReadonly = true;
                }

                _memoryCache.Set(QueriesDocumentCacheKey, queries, changeToken);
            }

            return queries;
        }

        public Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
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
