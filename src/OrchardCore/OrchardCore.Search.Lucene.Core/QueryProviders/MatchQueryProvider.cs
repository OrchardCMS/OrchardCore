using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class MatchQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "match")
        {
            return null;
        }

        var first = query.First();

        var boolQuery = new BooleanQuery();

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                foreach (var term in LuceneQueryService.Tokenize(first.Key, first.Value.ToString(), context.DefaultAnalyzer))
                {
                    boolQuery.Add(new TermQuery(new Term(first.Key, term)), Occur.SHOULD);
                }

                return boolQuery;

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
                            return new MatchAllDocsQuery();
                        }
                    }
                }

                foreach (var term in terms)
                {
                    boolQuery.Add(new TermQuery(new Term(first.Key, term)), occur);
                }

                return boolQuery;
            default: throw new ArgumentException("Invalid query");
        }
    }
}
