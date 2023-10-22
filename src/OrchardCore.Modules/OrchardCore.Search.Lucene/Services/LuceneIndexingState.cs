using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Search.Lucene
{
    /// <summary>
    /// This class persists the indexing state, a cursor, on the filesystem alongside the index itself.
    /// This state has to be on the filesystem as each node has its own local storage for the index.
    /// </summary>
    public class LuceneIndexingState
    {
        private readonly string _indexSettingsFilename;
        private readonly JsonObject _content;

        public LuceneIndexingState(
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _indexSettingsFilename = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name,
                "lucene.status.json");

            if (!File.Exists(_indexSettingsFilename))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_indexSettingsFilename));

                File.WriteAllText(_indexSettingsFilename, "{}");
            }

            _content = JsonNode.Parse(File.ReadAllText(_indexSettingsFilename))?.AsObject() ?? new JsonObject();
        }

        public long GetLastTaskId(string indexName)
        {
            JsonNode value;
            if (_content.TryGetPropertyValue(indexName, out value) && value != null)
            {
                return value.GetValue<long>();
            }
            else
            {
                lock (this)
                {
                    _content.Add(indexName, 0);
                }

                return 0L;
            }
        }

        public void SetLastTaskId(string indexName, long taskId)
        {
            lock (this)
            {
                _content[indexName] = taskId;
            }
        }

        public void Update()
        {
            lock (this)
            {
                File.WriteAllText(
                    _indexSettingsFilename,
                    JsonSerializer.Serialize(_content, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}
