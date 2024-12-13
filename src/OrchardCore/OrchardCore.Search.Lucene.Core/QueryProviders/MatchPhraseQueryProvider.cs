using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class MatchPhraseQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "match_phrase")
        {
            return null;
        }

        var first = query.First();

        var phraseQuery = new PhraseQuery();
        JsonNode value;

        switch (first.Value.GetValueKind())
        {
            case JsonValueKind.String:
                value = first.Value;
                break;

            case JsonValueKind.Object:
                var obj = first.Value.AsObject();

                if (!obj.TryGetPropertyValue("value", out value))
                {
                    throw new ArgumentException("Missing value in match phrase query");
                }

                // TODO: read "analyzer" property

                if (obj.TryGetPropertyValue("slop", out var slop))
                {
                    phraseQuery.Slop = slop.Value<int>();
                }

                break;
            default: throw new ArgumentException("Invalid wildcard query");
        }

        foreach (var term in LuceneQueryService.Tokenize(first.Key, value.Value<string>(), context.DefaultAnalyzer))
        {
            phraseQuery.Add(new Term(first.Key, term));
        }

        return phraseQuery;
    }
}
