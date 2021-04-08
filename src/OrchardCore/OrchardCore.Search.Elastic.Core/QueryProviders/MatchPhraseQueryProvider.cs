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
    public class MatchPhraseQueryProvider : IElasticQueryProvider
    {
        public Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query)
        {
            if (type != "match_phrase")
            {
                return null;
            }

            var first = query.Properties().First();

            var phraseQuery = new PhraseQuery();
            JToken value;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    value = first.Value;
                    break;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out value))
                    {
                        throw new ArgumentException("Missing value in match phrase query");
                    }

                    // TODO: read "analyzer" property

                    if (obj.TryGetValue("slop", out var slop))
                    {
                        phraseQuery.Slop = slop.Value<int>();
                    }

                    break;
                default: throw new ArgumentException("Invalid wildcard query");
            }

            foreach (var term in ElasticQueryService.Tokenize(first.Name, value.Value<string>(), context.DefaultAnalyzer))
            {
                phraseQuery.Add(new Term(first.Name, term));
            }

            return phraseQuery;
        }
    }
}
