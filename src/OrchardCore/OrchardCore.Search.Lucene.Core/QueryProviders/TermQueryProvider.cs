using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class TermQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "term")
            {
                return null;
            }

            var first = query.Properties().First();

            // A term query has only one member, which can either be a string or an object

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new TermQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    var value = obj.Property("value").Value.Value<string>();
                    var termQuery = new TermQuery(new Term(first.Name, value));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        termQuery.Boost = boost.Value<float>();
                    }

                    return termQuery;
                default: throw new ArgumentException("Invalid term query");
            }
        }
    }
}
