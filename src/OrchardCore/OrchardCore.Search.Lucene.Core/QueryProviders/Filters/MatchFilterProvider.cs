using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters;

public class MatchFilterProvider : ILuceneBooleanFilterProvider
{
    public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonNode filter, Query toFilter)
    {
        if (type != "match")
        {
            return null;
        }

        if (toFilter is not BooleanQuery booleanQuery)
        {
            return null;
        }

        var queryObj = filter.AsObject();
        var first = queryObj.First();

        var boolQuery = new BooleanQuery();
        QueryWrapperFilter queryFilter;

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                foreach (var term in LuceneQueryService.Tokenize(first.Key, first.Value.ToString(), context.DefaultAnalyzer))
                {
                    boolQuery.Add(new TermQuery(new Term(first.Key, term)), Occur.SHOULD);
                }
                break;
            case JsonValueKind.Object:
                var obj = first.Value.AsObject();
                var value = obj["query"]?.Value<string>();

                if (obj.TryGetPropertyValue("boost", out var boost))
                {
                    boolQuery.Boost = boost.Value<float>();
                }

                var occur = Occur.SHOULD;
                if (obj.TryGetPropertyValue("operator", out var op))
                {
                    occur = op.ToString() == "and" ? Occur.MUST : Occur.SHOULD;
                }

                var terms = LuceneQueryService.Tokenize(first.Key, value, context.DefaultAnalyzer);

                if (terms.Count == 0)
                {
                    if (obj.TryGetPropertyValue("zero_terms_query", out var zeroTermsQuery))
                    {
                        if (zeroTermsQuery.ToString() == "all")
                        {
                            var matchAllDocsQuery = new MatchAllDocsQuery();
                            booleanQuery.Add(matchAllDocsQuery, Occur.MUST);
                            break;
                        }
                    }
                }

                foreach (var term in terms)
                {
                    boolQuery.Add(new TermQuery(new Term(first.Key, term)), occur);
                }

                break;
            default: throw new ArgumentException("Invalid query");
        }

        booleanQuery.Add(boolQuery, Occur.MUST);
        queryFilter = new QueryWrapperFilter(boolQuery);

        return new FilteredQuery(booleanQuery, queryFilter);
    }
}
