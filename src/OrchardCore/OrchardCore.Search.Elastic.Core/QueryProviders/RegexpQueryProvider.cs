using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Elastic.QueryProviders
{

    /// <summary>
    /// We may not need this at all as Elastic has full blown DSL with NEST OOTB
    /// </summary>
    public class RegexpQueryProvider : IElasticQueryProvider
    {
        public Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query)
        {
            if (type != "regexp")
            {
                return null;
            }

            var first = query.Properties().First();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new RegexpQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in regexp query");
                    }

                    // TODO: Support flags

                    var regexpQuery = new RegexpQuery(new Term(first.Name, value.Value<string>()));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        regexpQuery.Boost = boost.Value<float>();
                    }

                    return regexpQuery;
                default: throw new ArgumentException("Invalid regexp query");
            }
        }
    }
}
