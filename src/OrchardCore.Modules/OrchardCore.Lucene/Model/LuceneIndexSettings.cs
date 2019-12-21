using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Lucene.Model
{
    public class LuceneIndexSettings
    {
        [JsonIgnore]
        public string IndexName { get; set; }

        public string AnalyzerName { get; set; }

        public bool IndexLatest { get; set; }

        public string[] IndexedContentTypes { get; set; }
    }

    public class LuceneIndexSettingsDocument
    {
        public Dictionary<string, LuceneIndexSettings> LuceneIndexSettings { get; set; } = new Dictionary<string, LuceneIndexSettings>();
    }
}
