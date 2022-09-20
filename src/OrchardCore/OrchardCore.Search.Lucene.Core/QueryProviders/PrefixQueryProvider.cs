using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class PrefixQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "prefix")
            {
                return null;
            }

            var first = query.Properties().First();

            // A prefix query has only one member, which can either be a string or an object

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new PrefixQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    PrefixQuery prefixQuery = null;

                    if (obj.TryGetValue("value", out var value))
                    {
                        prefixQuery = new PrefixQuery(new Term(first.Name, value.Value<string>()));
                    }
                    else if (obj.TryGetValue("prefix", out var prefix))
                    {
                        prefixQuery = new PrefixQuery(new Term(first.Name, prefix.Value<string>()));
                    }
                    else
                    {
                        throw new ArgumentException("Prefix query misses prefix value");
                    }

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        prefixQuery.Boost = boost.Value<float>();
                    }

                    return prefixQuery;
                default: throw new ArgumentException("Invalid prefix query");
            }
        }
    }
}
