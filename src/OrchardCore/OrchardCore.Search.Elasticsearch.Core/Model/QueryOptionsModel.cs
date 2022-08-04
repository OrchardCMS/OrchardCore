using System.Text.Json.Serialization;

namespace OrchardCore.Search.Elasticsearch.Model
{
    public class QueryOptionsModel
    {
        [JsonPropertyName("_source")]
        public bool? Source { get; set; } = true;

        [JsonPropertyName("size")]
        public int? Size { get; set; }

        [JsonPropertyName("from")]
        public int? From { get; set; } = 0;

        [JsonPropertyName("fields")]
        public string[] Fields { get; set; }
    }
}
