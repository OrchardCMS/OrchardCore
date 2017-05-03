using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace Orchard.Lucene.QueryProviders
{
    public class RootQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(IQueryDslBuilder builder, LuceneQueryContext context, string type, JObject jobj)
        {
            if (type != "query")
            {
                return null;
            }

            return  builder.CreateQuery(context, jobj);
        }
    }
}
