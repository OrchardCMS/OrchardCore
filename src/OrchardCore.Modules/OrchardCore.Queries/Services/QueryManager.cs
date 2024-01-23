using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Documents;

namespace OrchardCore.Queries.Services
{
    public class QueryManager : IQueryManager
    {
        private readonly IDocumentManager<QueriesDocument> _documentManager;
        private readonly IEnumerable<IQuerySource> _querySources;

        public QueryManager(IDocumentManager<QueriesDocument> documentManager, IEnumerable<IQuerySource> querySources)
        {
            _documentManager = documentManager;
            _querySources = querySources;
        }

        public async Task<string> GetIdentifierAsync() => (await GetDocumentAsync()).Identifier;

        public async Task DeleteQueryAsync(string name)
        {
            var existing = await LoadDocumentAsync();
            existing.Queries.Remove(name);
            await _documentManager.UpdateAsync(existing);
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
            var existing = await LoadDocumentAsync();
            existing.Queries.Remove(name);
            existing.Queries[query.Name] = query;
            await _documentManager.UpdateAsync(existing);
        }

        /// <summary>
        /// Loads the queries document from the store for updating and that should not be cached.
        /// </summary>
        public Task<QueriesDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the queries document from the cache for sharing and that should not be updated.
        /// </summary>
        public Task<QueriesDocument> GetDocumentAsync() => _documentManager.GetOrCreateImmutableAsync();

        public Task<IQueryResults> ExecuteQueryAsync(Query query, IDictionary<string, object> parameters)
        {
            var querySource = _querySources.FirstOrDefault(q => q.Name == query.Source)
                ?? throw new ArgumentException("Query source not found: " + query.Source);

            return querySource.ExecuteQueryAsync(query, parameters);
        }
    }
}
