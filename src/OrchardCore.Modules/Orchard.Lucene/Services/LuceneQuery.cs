using Newtonsoft.Json.Linq;
using Orchard.Queries;

namespace Orchard.Lucene
{
    public class LuceneQuery : Query
    {
        public string IndexName { get; set; }
        public JObject Content { get; set; }
    }
}
