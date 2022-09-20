using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class WildcardQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "wildcard")
            {
                return null;
            }

            var first = query.Properties().First();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new WildcardQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in wildcard query");
                    }

                    var wildCardQuery = new WildcardQuery(new Term(first.Name, value.Value<string>()));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        wildCardQuery.Boost = boost.Value<float>();
                    }

                    return wildCardQuery;
                default: throw new ArgumentException("Invalid wildcard query");
            }
        }
    }
}
