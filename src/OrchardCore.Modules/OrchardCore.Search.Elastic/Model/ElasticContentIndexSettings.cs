using OrchardCore.Indexing;

namespace OrchardCore.Search.Elastic.Model
{
    public class ElasticContentIndexSettings : IContentIndexSettings
    {
        public bool Included { get; set; }

        public DocumentIndexOptions ToOptions()
        {
            return DocumentIndexOptions.None;
        }
    }
}
