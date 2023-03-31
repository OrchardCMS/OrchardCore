using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Documents;
using OrchardCore.Search.Lucene.Model;

namespace OrchardCore.Search.Lucene
{
    public class LuceneIndexSettingsService
    {
        private readonly IDocumentManager<LuceneIndexSettingsDocument> _documentManager;

        public LuceneIndexSettingsService(IDocumentManager<LuceneIndexSettingsDocument> documentManager)
        {
            _documentManager = documentManager;
        }

        /// <summary>
        /// Loads the index settings document from the store for updating and that should not be cached.
        /// </summary>
        public Task<LuceneIndexSettingsDocument> LoadDocumentAsync() => _documentManager.GetOrCreateMutableAsync();

        /// <summary>
        /// Gets the index settings document from the cache for sharing and that should not be updated.
        /// </summary>
        public async Task<LuceneIndexSettingsDocument> GetDocumentAsync()
        {
            var document = await _documentManager.GetOrCreateImmutableAsync();

            foreach (var name in document.LuceneIndexSettings.Keys)
            {
                document.LuceneIndexSettings[name].IndexName = name;
            }

            return document;
        }

        public async Task<IEnumerable<LuceneIndexSettings>> GetSettingsAsync()
        {
            return (await GetDocumentAsync()).LuceneIndexSettings.Values;
        }

        public async Task<LuceneIndexSettings> GetSettingsAsync(string indexName)
        {
            var document = await GetDocumentAsync();

            if (document.LuceneIndexSettings.TryGetValue(indexName, out var settings))
            {
                return settings;
            }

            return null;
        }

        public async Task<string> GetIndexAnalyzerAsync(string indexName)
        {
            var document = await GetDocumentAsync();

            if (document.LuceneIndexSettings.TryGetValue(indexName, out var settings))
            {
                return settings.AnalyzerName;
            }

            return LuceneSettings.StandardAnalyzer;
        }

        public async Task<string> LoadIndexAnalyzerAsync(string indexName)
        {
            var document = await LoadDocumentAsync();

            if (document.LuceneIndexSettings.TryGetValue(indexName, out var settings))
            {
                return settings.AnalyzerName;
            }

            return LuceneSettings.StandardAnalyzer;
        }

        public async Task UpdateIndexAsync(LuceneIndexSettings settings)
        {
            var document = await LoadDocumentAsync();
            document.LuceneIndexSettings[settings.IndexName] = settings;
            await _documentManager.UpdateAsync(document);
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            var document = await LoadDocumentAsync();
            document.LuceneIndexSettings.Remove(indexName);
            await _documentManager.UpdateAsync(document);
        }
    }
}
