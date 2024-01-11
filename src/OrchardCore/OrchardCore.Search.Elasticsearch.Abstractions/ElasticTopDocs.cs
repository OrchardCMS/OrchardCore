using System.Collections.Generic;

namespace OrchardCore.Search.Elasticsearch
{
    public class ElasticTopDocs
    {
        public List<Dictionary<string, object>> TopDocs { get; set; }
        public List<Dictionary<string, object>> Fields { get; set; }
        public long Count { get; set; }
    }
}
