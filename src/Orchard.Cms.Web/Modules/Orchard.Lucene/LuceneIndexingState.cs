using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Orchard.Environment.Shell;

namespace Orchard.Lucene
{
    /// <summary>
    /// This class persists the indexing state, a cursor, on the filesystem alongside the index itself.
    /// This state has to be on the filesystem as each node has its own local storage for the index.
    /// </summary>
    public class LuceneIndexingState
    {
        private readonly string _indexSettingsFilename;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly JObject _content;

        public LuceneIndexingState(
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings
            )
        {
            _hostingEnvironment = hostingEnvironment;

            _indexSettingsFilename = Path.Combine(
                shellOptions.Value.ShellsRootContainerName, 
                shellOptions.Value.ShellsContainerName, 
                shellSettings.Name, 
                "lucene.status.json");

            if (!File.Exists(_indexSettingsFilename))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_indexSettingsFilename));

                File.WriteAllText(_indexSettingsFilename, new JObject().ToString(Newtonsoft.Json.Formatting.Indented));
            }

            _content = JObject.Parse(File.ReadAllText(_indexSettingsFilename));
        }

        public int GetLastTaskId(string indexName)
        {
            JToken value;
            if (_content.TryGetValue(indexName, out value))
            {
                return value.Value<int>();
            }
            else
            {
                _content.Add(new JProperty(indexName, 0));
                return 0;
            }
        }

        public void SetLastTaskId(string indexName, int taskId)
        {
            _content[indexName] = taskId;
        }

        public void Update()
        {
            File.WriteAllText(_indexSettingsFilename, _content.ToString(Newtonsoft.Json.Formatting.Indented));
        }
    }
}
