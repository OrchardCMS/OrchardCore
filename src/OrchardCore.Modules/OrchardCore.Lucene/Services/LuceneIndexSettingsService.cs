using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using OrchardCore.Data;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Lucene.Model;
using YesSql;

namespace OrchardCore.Lucene
{
    public class LuceneIndexSettingsService
    {
        private const string CacheKey = nameof(LuceneIndexSettingsService);

        private readonly ISignal _signal;

        private LuceneIndexSettingsDocument _document;
        private IChangeToken _changeToken;

        public LuceneIndexSettingsService(ISignal signal)
        {
            _signal = signal;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <summary>
        /// Returns the document from the database to be updated.
        /// </summary>
        public async Task<LuceneIndexSettingsDocument> LoadDocumentAsync()
        {
            return await SessionHelper.LoadForUpdateAsync<LuceneIndexSettingsDocument>();
        }

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        private async Task<LuceneIndexSettingsDocument> GetDocumentAsync()
        {
            if (_document == null || (_changeToken?.HasChanged ?? true))
            {
                _changeToken = ChangeToken;

                var document = await SessionHelper.GetForCachingAsync<LuceneIndexSettingsDocument>();

                foreach (var name in document.LuceneIndexSettings.Keys)
                {
                    document.LuceneIndexSettings[name].IndexName = name;
                    document.LuceneIndexSettings[name].IsReadonly = true;
                }

                _document = document;
            }

            return _document;
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
            if (settings.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            var document = await LoadDocumentAsync();
            document.LuceneIndexSettings[settings.IndexName] = settings;

            Session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            var document = await LoadDocumentAsync();
            document.LuceneIndexSettings.Remove(indexName);

            Session.Save(document);
            _signal.DeferredSignalToken(CacheKey);
        }

        private ISession Session => ShellScope.Services.GetRequiredService<ISession>();
        private ISessionHelper SessionHelper => ShellScope.Services.GetRequiredService<ISessionHelper>();
    }
}
