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
    public class TermQueryProvider : IElasticQueryProvider
    {
        public Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query)
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
