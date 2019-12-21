using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Lucene.Model;
using YesSql;

namespace OrchardCore.Lucene
{
    /// <summary>
    /// This class persists the indexing state, a cursor, on the filesystem alongside the index itself.
    /// This state has to be on the filesystem as each node has its own local storage for the index.
    /// </summary>
    public class LuceneIndexSettingsService
    {
        private readonly IStore _store;
        private ImmutableDictionary<string, LuceneIndexSettings> _indexSettings;

        public LuceneIndexSettingsService(IStore store)
        {
            _store = store;
        }

        public async Task<IEnumerable<LuceneIndexSettings>> GetSettingsAsync()
        {
            await EnsureSettingsLoaded();
            return _indexSettings.Values;
        }

        private async Task EnsureSettingsLoaded()
        {
            using (var session = _store.CreateSession())
            {
                if (_indexSettings == null)
                {
                    var document = await session.Query<LuceneIndexSettingsDocument>().FirstOrDefaultAsync();

                    if (document == null)
                    {
                        _indexSettings = ImmutableDictionary<string, LuceneIndexSettings>.Empty;
                    }
                    else
                    {
                        _indexSettings = document.LuceneIndexSettings.ToImmutableDictionary();

                        foreach (var name in _indexSettings.Keys)
                        {
                            _indexSettings[name].IndexName = name;
                        }
                    }
                }
            }
        }

        public async Task<LuceneIndexSettings> GetSettingsAsync(string indexName)
        {
            await EnsureSettingsLoaded();

            if (_indexSettings.TryGetValue(indexName, out var settings))
            {
                return settings;
            }

            return null;
        }

        public async Task<string> GetIndexAnalyzerAsync(string indexName)
        {
            await EnsureSettingsLoaded();

            if (_indexSettings.TryGetValue(indexName, out var settings))
            {
                return settings.AnalyzerName;
            }

            return LuceneSettings.StandardAnalyzer;
        }

        public async Task UpdateIndexAsync(LuceneIndexSettings settings)
        {
            await EnsureSettingsLoaded();

            _indexSettings = _indexSettings.SetItem(settings.IndexName, settings);

            await SaveAsync();
        }

        public async Task DeleteIndexAsync(string indexName)
        {
            await EnsureSettingsLoaded();

            _indexSettings = _indexSettings.Remove(indexName);

            await SaveAsync();
        }

        private async Task SaveAsync()
        {
            using (var session = _store.CreateSession())
            {
                var document = await session.Query<LuceneIndexSettingsDocument>().FirstOrDefaultAsync();
                if (document == null)
                {
                    document = new LuceneIndexSettingsDocument();
                }

                document.LuceneIndexSettings = new Dictionary<string, LuceneIndexSettings>(_indexSettings);

                session.Save(document);
                await session.CommitAsync();
            }
        }
    }
}
