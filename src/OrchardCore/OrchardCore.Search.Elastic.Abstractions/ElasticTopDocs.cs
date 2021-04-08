using System.Collections.Generic;

namespace OrchardCore.Search.Elastic
{
    public class ElasticTopDocs
    {
        public List<ElasticDocument> TopDocs { get; set; }
        public int Count { get; set; }
    }
}
