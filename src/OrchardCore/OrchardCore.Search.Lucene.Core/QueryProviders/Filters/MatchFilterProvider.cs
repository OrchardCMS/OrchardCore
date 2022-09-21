using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters
{
    public class MatchFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "match")
            {
                return null;
            }

            if (!(toFilter is BooleanQuery booleanQuery))
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            var boolQuery = new BooleanQuery();
            QueryWrapperFilter queryFilter;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    foreach (var term in LuceneQueryService.Tokenize(first.Name, first.Value.ToString(), context.DefaultAnalyzer))
                    {
                        boolQuery.Add(new TermQuery(new Term(first.Name, term)), Occur.SHOULD);
                    }
                    break;
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
                                var matchAllDocsQuery = new MatchAllDocsQuery();
                                booleanQuery.Add(matchAllDocsQuery, Occur.MUST);
                                queryFilter = new QueryWrapperFilter(matchAllDocsQuery);
                                break;
                            }
                        }
                    }

                    foreach (var term in terms)
                    {
                        boolQuery.Add(new TermQuery(new Term(first.Name, term)), occur);
                    }

                    break;
                default: throw new ArgumentException("Invalid query");
            }

            booleanQuery.Add(boolQuery, Occur.MUST);
            queryFilter = new QueryWrapperFilter(boolQuery);

            return new FilteredQuery(booleanQuery, queryFilter);
        }
    }
}
