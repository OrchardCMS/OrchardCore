using OrchardCore.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ElasticContentIndexSettings : IContentIndexSettings
    {
        public bool Included { get; set; }

        public DocumentIndexOptions ToOptions()
        {
            var options = DocumentIndexOptions.None;

            return options;
        }
    }
}
