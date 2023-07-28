using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders
{
    public class MatchQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "match")
            {
                return null;
            }

            var first = query.Properties().First();

            var boolQuery = new BooleanQuery();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    foreach (var term in LuceneQueryService.Tokenize(first.Name, first.Value.ToString(), context.DefaultAnalyzer))
                    {
                        boolQuery.Add(new TermQuery(new Term(first.Name, term)), Occur.SHOULD);
                    }
                    return boolQuery;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    var value = obj.Property("query")?.Value.Value<string>();

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        boolQuery.Boost = boost.Value<float>();
                    }

                    var occur = Occur.SHOULD;
                    if (obj.TryGetValue("operator", out var op))
                    {
                        occur = op.ToString() == "and" ? Occur.MUST : Occur.SHOULD;
                    }

                    var terms = LuceneQueryService.Tokenize(first.Name, value, context.DefaultAnalyzer);

                    if (!terms.Any())
                    {
                        if (obj.TryGetValue("zero_terms_query", out var zeroTermsQuery))
                        {
                            if (zeroTermsQuery.ToString() == "all")
                            {
                                return new MatchAllDocsQuery();
                            }
                        }
                    }

                    foreach (var term in terms)
                    {
                        boolQuery.Add(new TermQuery(new Term(first.Name, term)), occur);
                    }

                    return boolQuery;
                default: throw new ArgumentException("Invalid query");
            }
        }
    }
}
