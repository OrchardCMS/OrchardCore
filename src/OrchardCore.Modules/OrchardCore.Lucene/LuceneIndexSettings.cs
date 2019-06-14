using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Lucene
{
    /// <summary>
    /// This class persists the indexing state, a cursor, on the filesystem alongside the index itself.
    /// This state has to be on the filesystem as each node has its own local storage for the index.
    /// </summary>
    public class LuceneIndexSettings
    {
        private readonly string _indexSettingsFilename;
        private readonly JObject _content;

        public LuceneIndexSettings(
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

                File.WriteAllText(_indexSettingsFilename, new JObject().ToString(Newtonsoft.Json.Formatting.Indented));
            }

            _content = JObject.Parse(File.ReadAllText(_indexSettingsFilename));
        }

        public IEnumerable<IndexSettings> List() {
            return _content.ToObject<IEnumerable<IndexSettings>>();
        }

        public string GetIndexAnalyzer(string indexName)
        {
            JToken value;
            if (_content.TryGetValue(indexName, out value))
            {
                return value.Value<string>();
            }
            else
            {
                lock (this)
                {
                    _content.Add(new JProperty(indexName, "standardanalyzer"));
                }

                return "standardanalyzer";
            }
        }

        public void CreateIndex(string indexName, string analyzerName)
        {
            lock (this)
            {
                _content[indexName] = analyzerName;
                Update();
            }
        }

        public void DeleteIndex(string indexName)
        {
            lock (this)
            {
                _content.Remove(indexName);
                Update();
            }
        }

        private void Update()
        {
            lock (this)
            {
                File.WriteAllText(_indexSettingsFilename, _content.ToString(Newtonsoft.Json.Formatting.Indented));
            }
        }
    }
}
