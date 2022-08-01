using System.Text.Json.Serialization;

namespace OrchardCore.Search.Elastic.Model
{
    public class QueryOptionsModel
    {
        [JsonPropertyName("_source")]
        public bool? Source { get; set; } = true;

        [JsonPropertyName("size")]
        public int? Size { get; set; }

        [JsonPropertyName("from")]
        public int? From { get; set; }

        [JsonPropertyName("fields")]
        public string[] Fields { get; set; }

        [JsonPropertyName("sort")]
        public string Sort { get; set; }
    }

    public class SortOptions
    {
        [JsonPropertyName("order")]
        public string Order { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }
    }
}
