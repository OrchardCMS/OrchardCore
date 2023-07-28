using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch.Core.Services
{
    public class ElasticIndexSettingsService
    {
        /// <summary>
        /// Loads the index settings document from the store for updating and that should not be cached.
        /// </summary>
#pragma warning disable CA1822 // Mark members as static
        public Task<ElasticIndexSettingsDocument> LoadDocumentAsync() => DocumentManager.GetOrCreateMutableAsync();
#pragma warning restore CA1822 // Mark members as static

        /// <summary>
        /// Gets the index settings document from the cache for sharing and that should not be updated.
        /// </summary>
#pragma warning disable CA1822 // Mark members as static
        public async Task<ElasticIndexSettingsDocument> GetDocumentAsync()
#pragma warning restore CA1822 // Mark members as static
        {
            var document = await DocumentManager.GetOrCreateImmutableAsync();

            foreach (var name in document.ElasticIndexSettings.Keys)
            {
                document.ElasticIndexSettings[name].IndexName = name;
            }

            return document;
        }

        public async Task<IEnumerable<ElasticIndexSettings>> GetSettingsAsync()
        {
            return (await GetDocumentAsync()).ElasticIndexSettings.Values;
        }

        public async Task<ElasticIndexSettings> GetSettingsAsync(string indexName)
        {
            var document = await GetDocumentAsync();

            if (document.ElasticIndexSettings.TryGetValue(indexName, out var settings))
            {
                return settings;
            }

            return null;
        }

        /// <summary>
        /// Returns the name of he index-time analyzer.
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public async Task<string> GetIndexAnalyzerAsync(string indexName)
        {
            var document = await GetDocumentAsync();

            return GetAnalyzerName(document, indexName);
        }

        /// <summary>
        /// Returns the name of the query-time analyzer.
        /// </summary>
        /// <param name="indexName"></param>
        /// <returns></returns>
        public async Task<string> GetQueryAnalyzerAsync(string indexName)
        {
            var document = await GetDocumentAsync();

            if (document.ElasticIndexSettings.TryGetValue(indexName, out var settings) && !String.IsNullOrEmpty(settings.QueryAnalyzerName))
            {
                return settings.QueryAnalyzerName;
            }

            return ElasticsearchConstants.DefaultAnalyzer;
        }

        public async Task<string> LoadIndexAnalyzerAsync(string indexName)
        {
            var document = await LoadDocumentAsync();

            return GetAnalyzerName(document, indexName);
        }

        public async Task UpdateIndexAsync(ElasticIndexSettings settings)
        {
            var document = await LoadDocumentAsync();
            document.ElasticIndexSettings[settings.IndexName] = settings;
            await DocumentManager.UpdateAsync(document);
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            var document = await LoadDocumentAsync();
            document.ElasticIndexSettings.Remove(indexName);
            await DocumentManager.UpdateAsync(document);
        }

        private static IDocumentManager<ElasticIndexSettingsDocument> DocumentManager =>
            ShellScope.Services.GetRequiredService<IDocumentManager<ElasticIndexSettingsDocument>>();

        // Returns the name of the analyzer configured for the given index name.
        private static string GetAnalyzerName(ElasticIndexSettingsDocument document, string indexName)
        {
            // The name "standardanalyzer" is a legacy used prior OC 1.6 release. It can be removed in future releases.
            if (document.ElasticIndexSettings.TryGetValue(indexName, out var settings) && settings.AnalyzerName != "standardanalyzer")
            {
                return settings.AnalyzerName;
            }

            return ElasticsearchConstants.DefaultAnalyzer;
        }
    }
}
