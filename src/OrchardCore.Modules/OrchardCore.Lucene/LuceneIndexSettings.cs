using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
        private readonly List<IndexSettings> _indexSettings;
        //private readonly JObject _content;

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
                File.WriteAllText(_indexSettingsFilename, "");
                //_indexSettings.Add(new IndexSettings { IndexName = "default", AnalyzerName = "standardanalyzer" });

                //Update();
            }

            _indexSettings = JsonConvert.DeserializeObject<List<IndexSettings>>(File.ReadAllText(_indexSettingsFilename)) ?? new List<IndexSettings>();
        }

        public IEnumerable<IndexSettings> List() {
            return _indexSettings;
        }

        public string GetIndexAnalyzer(string indexName)
        {
            var setting = _indexSettings.Where(x => x.IndexName == indexName).FirstOrDefault();
            if (setting != null)
            {
                return setting.AnalyzerName;
            }
            else
            {
                lock (this)
                {
                    _indexSettings.Add(new IndexSettings { IndexName = "default", AnalyzerName = "standardanalyzer" });
                }

                return "standardanalyzer";
            }
        }

        public void EditIndex(IndexSettings settings)
        {
            lock (this)
            {
                _indexSettings.Remove(settings);
                _indexSettings.Add(settings);
                Update();
            }
        }

        public void CreateIndex(IndexSettings settings)
        {
            lock (this)
            {

                _indexSettings.Add(settings);
                Update();
            }
        }

        public void DeleteIndex(IndexSettings settings)
        {
            lock (this)
            {
                _indexSettings.Remove(settings);
                Update();
            }
        }

        private void Update()
        {
            lock (this)
            {
                File.WriteAllText(_indexSettingsFilename, JsonConvert.SerializeObject(_indexSettings, Formatting.Indented));
            }
        }
    }
}
