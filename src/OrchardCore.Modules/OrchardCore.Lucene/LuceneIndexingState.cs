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
    public class LuceneIndexingState
    {
        private readonly string _rootPath;
        private readonly JObject _content = new JObject();

        public LuceneIndexingState(
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _rootPath = PathExtensions.Combine(
                shellOptions.Value.ShellsApplicationDataPath,
                shellOptions.Value.ShellsContainerName,
                shellSettings.Name, "Lucene");
        }

        public int GetLastTaskId(string indexName)
        {
            if (!_content.TryGetValue(indexName, out var value))
            {
                var statusFile = PathExtensions.Combine(_rootPath, indexName, "status.json");

                lock (this)
                {
                    if (File.Exists(statusFile))
                    {
                        value = _content[indexName] = JObject.Parse(File.ReadAllText(statusFile));
                    }
                    else
                    {
                        value = _content[indexName] = new JObject(new JProperty("taskId", 0));
                    }
                }
            }

            return value["taskId"]?.Value<int>() ?? 0;
        }

        public void SetLastTaskId(string indexName, int taskId)
        {
            lock (this)
            {
                if (!_content.ContainsKey(indexName))
                {
                    _content[indexName] = new JObject(new JProperty("taskId", 0));
                }

                _content[indexName]["taskId"] = taskId;
            }
        }

        public void Update(string indexName)
        {
            var statusFile = PathExtensions.Combine(_rootPath, indexName, "status.json");

            if (!Directory.Exists(Path.GetDirectoryName(statusFile)))
            {
                return;
            }

            lock (this)
            {
                if (!_content.ContainsKey(indexName))
                {
                    _content[indexName] = new JObject(new JProperty("taskId", 0));
                }

                File.WriteAllText(statusFile, _content[indexName].ToString(Newtonsoft.Json.Formatting.Indented));
            }
        }
    }
}
