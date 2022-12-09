using System.Collections.Generic;
using Newtonsoft.Json;
using OrchardCore.Data.Documents;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    public class ElasticIndexSettings
    {
        [JsonIgnore]
        public string IndexName { get; set; }

        public string AnalyzerName { get; set; }

        public bool IndexLatest { get; set; }

        public string[] IndexedContentTypes { get; set; }

        public string Culture { get; set; }

        public bool StoreSourceData { get; set; } = true;
    }

    public class ElasticIndexSettingsDocument : Document
    {
        public Dictionary<string, ElasticIndexSettings> ElasticIndexSettings { get; set; } = new Dictionary<string, ElasticIndexSettings>();
    }
}
