using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrchardCore.Environment.Shell;
using OrchardCore.Lucene.Model;

namespace OrchardCore.Lucene
{
    /// <summary>
    /// This class persists the indexing state, a cursor, on the filesystem alongside the index itself.
    /// This state has to be on the filesystem as each node has its own local storage for the index.
    /// </summary>
    public class LuceneIndexSettingsService
    {
        private readonly string _indexSettingsFilename;
        private ImmutableDictionary<string, LuceneIndexSettings> _indexSettings =
            ImmutableDictionary.Create<string, LuceneIndexSettings>(StringComparer.OrdinalIgnoreCase);

        public LuceneIndexSettingsService(
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _indexSettingsFilename = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name,
                "lucene.settings.json");

            if (!File.Exists(_indexSettingsFilename))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_indexSettingsFilename));
                File.WriteAllText(_indexSettingsFilename, "");
            }

            var settings = JsonConvert.DeserializeObject<List<LuceneIndexSettings>>(File.ReadAllText(_indexSettingsFilename)) ?? new List<LuceneIndexSettings>();

            _indexSettings = ImmutableDictionary.Create<string, LuceneIndexSettings>(StringComparer.OrdinalIgnoreCase)
                .AddRange(settings.ToDictionary(s => s.IndexName, s => s));
        }

        public IEnumerable<string> GetIndices() => _indexSettings.Keys;

        public IEnumerable<LuceneIndexSettings> GetSettings() => _indexSettings.Values;

        public LuceneIndexSettings GetSettings(string indexName)
        {
            _indexSettings.TryGetValue(indexName, out var settings);
            return settings;
        }

        public string GetIndexAnalyzer(string indexName)
        {
            _indexSettings.TryGetValue(indexName, out var settings);
            return settings?.AnalyzerName ?? LuceneSettings.StandardAnalyzer;
        }

        public void UpdateIndex(LuceneIndexSettings settings)
        {
            lock (this)
            {
                _indexSettings = _indexSettings.SetItem(settings.IndexName, settings);
                Update();
            }
        }

        public void DeleteIndex(string indexName)
        {
            lock (this)
            {
                _indexSettings = _indexSettings.Remove(indexName);
                Update();
            }
        }

        private void Update()
        {
            lock (this)
            {
                File.WriteAllText(_indexSettingsFilename, JsonConvert.SerializeObject(GetSettings(), Formatting.Indented));
            }
        }
    }
}
