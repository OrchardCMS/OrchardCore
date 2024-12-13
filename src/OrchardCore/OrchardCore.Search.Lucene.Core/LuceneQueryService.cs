using System.Text.Json;
using System.Text.Json.Nodes;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Search;

namespace OrchardCore.Search.Lucene;

public class LuceneQueryService : ILuceneQueryService
{
    private readonly IEnumerable<ILuceneQueryProvider> _queryProviders;

    public LuceneQueryService(IEnumerable<ILuceneQueryProvider> queryProviders)
    {
        _queryProviders = queryProviders;
    }

    public Task<LuceneTopDocs> SearchAsync(LuceneQueryContext context, JsonObject queryObj)
    {
        var queryProp = queryObj["query"].AsObject()
            ?? throw new ArgumentException("Query DSL requires a [query] property");

        var query = CreateQueryFragment(context, queryProp);

        var sortProperty = queryObj["sort"];
        var fromProperty = queryObj["from"];
        var sizeProperty = queryObj["size"];

        var size = sizeProperty.ValueOrDefault<int>(10);
        var from = fromProperty.ValueOrDefault<int>(0);

        string sortField = null;
        string sortOrder = null;

        var sortFields = new List<SortField>();

        if (sortProperty is not null)
        {
            string sortType;

            if (sortProperty.GetValueKind() == JsonValueKind.String)
            {
                sortField = sortProperty.ToString();
                sortFields.Add(new SortField(sortField, SortFieldType.STRING, sortOrder == "desc"));
            }
            else if (sortProperty is JsonObject jsonObject)
            {
                sortField = jsonObject.First().Key;
                sortOrder = jsonObject.First().Value["order"].ToString();
                sortType = jsonObject.First().Value["type"]?.ToString();
                var sortFieldType = SortFieldType.STRING;

                if (sortType != null)
                {
                    sortFieldType = (SortFieldType)Enum.Parse(typeof(SortFieldType), sortType.ToUpper());
                }

                sortFields.Add(new SortField(sortField, sortFieldType, sortOrder == "desc"));
            }
            else if (sortProperty is JsonArray jsonArray)
            {
                foreach (var item in jsonArray)
                {
                    sortField = item.AsObject().First().Key;
                    sortOrder = item.AsObject().First().Value["order"].ToString();
                    sortType = item.AsObject().First().Value["type"]?.ToString();
                    var sortFieldType = SortFieldType.STRING;

                    if (sortType != null)
                    {
                        sortFieldType = (SortFieldType)Enum.Parse(typeof(SortFieldType), sortType.ToUpper());
                    }

                    sortFields.Add(new SortField(sortField, sortFieldType, sortOrder == "desc"));
                }
            }
        }

        LuceneTopDocs result = null;

        if (size > 0)
        {
            TopDocs topDocs = context.IndexSearcher.Search(
                query,
                size + from,
                sortField == null ? Sort.RELEVANCE : new Sort(sortFields.ToArray())
            );

            if (from > 0)
            {
                topDocs = new TopDocs(topDocs.TotalHits - from, topDocs.ScoreDocs.Skip(from).ToArray(), topDocs.MaxScore);
            }

            var collector = new TotalHitCountCollector();
            context.IndexSearcher.Search(query, collector);

            result = new LuceneTopDocs() { TopDocs = topDocs, Count = collector.TotalHits };
        }

        return Task.FromResult(result);
    }

    public Query CreateQueryFragment(LuceneQueryContext context, JsonObject queryObj)
    {
        var first = queryObj.First();

        Query query = null;

        foreach (var queryProvider in _queryProviders)
        {
            query = queryProvider.CreateQuery(this, context, first.Key, first.Value.AsObject());

            if (query != null)
            {
                break;
            }
        }

        return query;
    }

    public static List<string> Tokenize(string fieldName, string text, Analyzer analyzer)
    {
        if (string.IsNullOrEmpty(text))
        {
            return [];
        }

        var result = new List<string>();
        using (var tokenStream = analyzer.GetTokenStream(fieldName, text))
        {
            tokenStream.Reset();
            while (tokenStream.IncrementToken())
            {
                var termAttribute = tokenStream.GetAttribute<ICharTermAttribute>();

                if (termAttribute != null)
                {
                    result.Add(termAttribute.ToString());
                }
            }
        }

        return result;
    }
}
