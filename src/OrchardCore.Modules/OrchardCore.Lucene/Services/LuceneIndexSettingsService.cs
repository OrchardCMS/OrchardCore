using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Lucene.Model;
using YesSql;

namespace OrchardCore.Lucene
{
    public class LuceneIndexSettingsService
    {
        private const string CacheKey = nameof(LuceneIndexSettingsService);

        private readonly IStore _store;
        private readonly ISignal _signal;

        private LuceneIndexSettingsDocument _document;
        private IChangeToken _changeToken;

        public LuceneIndexSettingsService(IStore store, ISignal signal)
        {
            _store = store;
            _signal = signal;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        /// <summary>
        /// Returns the document from the cache or creates a new one. The result should not be updated.
        /// </summary>
        private async Task<LuceneIndexSettingsDocument> GetDocumentAsync()
        {
            if (_document == null || (_changeToken?.HasChanged ?? true))
            {
                _changeToken = ChangeToken;

                LuceneIndexSettingsDocument document;

                using (var session = _store.CreateSession())
                {
                    document = await LoadDocumentAsync(session);
                }

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

        public async Task UpdateIndexAsync(LuceneIndexSettings settings)
        {
            if (settings.IsReadonly)
            {
                throw new ArgumentException("The object is read-only");
            }

            using (var session = _store.CreateSession())
            {
                var document = await LoadDocumentAsync(session);
                document.LuceneIndexSettings[settings.IndexName] = settings;

                session.Save(document);
                await session.CommitAsync();
            }

            _signal.SignalToken(CacheKey);
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            using (var session = _store.CreateSession())
            {
                var document = await LoadDocumentAsync(session);
                document.LuceneIndexSettings.Remove(indexName);

                session.Save(document);
                await session.CommitAsync();
            }

            _signal.SignalToken(CacheKey);
        }

        private async Task<LuceneIndexSettingsDocument> LoadDocumentAsync(ISession session)
        {
            var document = await session.Query<LuceneIndexSettingsDocument>().FirstOrDefaultAsync();

            if (document == null)
            {
                document = new LuceneIndexSettingsDocument();
            }

            return document;
        }
    }
}
