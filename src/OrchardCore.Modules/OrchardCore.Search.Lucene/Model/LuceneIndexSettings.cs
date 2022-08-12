using System.Collections.Generic;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Search.Lucene.Model
{
    public class LuceneIndexSettings
    {
        [JsonIgnore]
        public string IndexName { get; set; }

        public string AnalyzerName { get; set; }

        public bool IndexLatest { get; set; }

        public string[] IndexedContentTypes { get; set; }

        public string Culture { get; set; }

        public bool StoreSourceData { get; set; }
    }

    public class LuceneIndexSettingsDocument : Document
    {
        public Dictionary<string, LuceneIndexSettings> LuceneIndexSettings { get; set; } = new Dictionary<string, LuceneIndexSettings>();
    }
}
