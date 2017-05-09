using Newtonsoft.Json.Linq;
using Orchard.Queries;

namespace Orchard.Lucene
{
    public class LuceneQuery : Query
    {
        public LuceneQuery() : base("Lucene")
        {
        }

        public string IndexName { get; set; }
        public JObject Content { get; set; }
    }
}
