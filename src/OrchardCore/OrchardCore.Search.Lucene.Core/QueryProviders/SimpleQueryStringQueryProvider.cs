using System.Text.Json.Nodes;
using Lucene.Net.QueryParsers.Simple;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene.QueryProviders;

public class SimpleQueryStringQueryProvider : ILuceneQueryProvider
{
    public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JsonObject query)
    {
        if (type != "simple_query_string")
        {
            return null;
        }

        var queryString = query["query"]?.Value<string>();
        var fields = query["fields"]?.Values<string>() ?? Array.Empty<string>();
        var defaultOperator = query["default_operator"]?.Value<string>().ToLowerInvariant() ?? "or";
        var weight = 1.0f;
        var weights = fields.ToDictionary(field => field, field => weight);

        foreach (var field in fields)
        {
            var fieldWeightArray = field.Split('^', 2);

            if (fieldWeightArray.Length > 1 && float.TryParse(fieldWeightArray[1], out weight))
            {
                weights.Remove(field);
                weights.Add(fieldWeightArray[0], weight);
            }
        }

        var queryParser = new SimpleQueryParser(context.DefaultAnalyzer, weights);
        switch (defaultOperator)
        {
            case "and":
                queryParser.DefaultOperator = Occur.MUST;
                break;
            case "or":
                queryParser.DefaultOperator = Occur.SHOULD;
                break;
        }
        return queryParser.Parse(queryString);
    }
}
