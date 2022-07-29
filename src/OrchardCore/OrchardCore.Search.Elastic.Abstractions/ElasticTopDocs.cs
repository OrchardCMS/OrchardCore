using System.Collections.Generic;
using Nest;

namespace OrchardCore.Search.Elastic
{
    public class ElasticTopDocs
    {
        public List<Dictionary<string, object>> TopDocs { get; set; }
        public List<Dictionary<string, object>> Fields { get; set; }
        public long Count { get; set; }
    }
}
