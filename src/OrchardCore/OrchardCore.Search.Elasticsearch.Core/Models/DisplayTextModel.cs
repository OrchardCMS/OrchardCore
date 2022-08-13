using Nest;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    internal class DisplayTextModel
    {
        [Text(Name = "Analyzed")]
        public string Analyzed { get; set; }

        [Keyword(Name = "Normalized")]
        public string Normalized { get; set; }

        [Keyword(Name = "keyword")]
        public string Keyword { get; set; }
    }
}
