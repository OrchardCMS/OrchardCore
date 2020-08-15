using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;
using OrchardCore.Lucene.FieldComparers;

namespace OrchardCore.Lucene
{
    public class LuceneQueryService : ILuceneQueryService
    {
        private readonly IEnumerable<ILuceneQueryProvider> _queryProviders;

        public LuceneQueryService(IEnumerable<ILuceneQueryProvider> queryProviders)
        {
            _queryProviders = queryProviders;
        }

        public Task<LuceneTopDocs> SearchAsync(LuceneQueryContext context, JObject queryObj)
        {
            var queryProp = queryObj["query"] as JObject;

            if (queryProp == null)
            {
                throw new ArgumentException("Query DSL requires a [query] property");
            }

            var query = CreateQueryFragment(context, queryProp);

            var sortProperty = queryObj["sort"];
            var fromProperty = queryObj["from"];
            var sizeProperty = queryObj["size"];

            var size = sizeProperty?.Value<int>() ?? 50;
            var from = fromProperty?.Value<int>() ?? 0;

            var sortFields = new SortField[0];

            if (sortProperty != null)
            {
                switch (sortProperty.Type)
                {
                    case JTokenType.String:
                        var sortField = sortProperty.ToString();
                        sortFields = new[] {new SortField(sortField, SortFieldType.STRING)};
                        break;
                    case JTokenType.Object:
                        sortFields = new[] {GetSortField(sortProperty)};
                        break;
                    case JTokenType.Array:
                        sortFields = sortProperty.Children().Select(GetSortField).ToArray();
                        break;
                }
            }

            TopDocs docs = context.IndexSearcher.Search(
                query,
                size + from,
                sortFields.Length == 0 ? Sort.RELEVANCE : new Sort(sortFields)
            );

            if (from > 0)
            {
                docs = new TopDocs(docs.TotalHits - from, docs.ScoreDocs.Skip(from).ToArray(), docs.MaxScore);
            }

            var collector = new TotalHitCountCollector();
            context.IndexSearcher.Search(query, collector);

            var result = new LuceneTopDocs { TopDocs = docs, Count = collector.TotalHits };

            return Task.FromResult(result);
        }

        private static SortField GetSortField(JToken item)
        {
            var sortField = ((JProperty)item.First).Name;
            var sortOrder = ((JProperty) item.First).Value["order"].ToString();
            var sortType = ((JProperty) item.First).Value["type"]?.ToString();

            if (sortOrder == "random")
            {
                return new SortField(sortField, new RandomFieldComparatorSource());
            }

            var sortFieldType = SortFieldType.STRING;
            if (sortType != null)
            {
                sortFieldType = (SortFieldType) Enum.Parse(typeof(SortFieldType), sortType.ToUpper());
            }

            return new SortField(sortField, sortFieldType, sortOrder == "desc");
        }

        public Query CreateQueryFragment(LuceneQueryContext context, JObject queryObj)
        {
            var first = queryObj.Properties().First();

            Query query = null;

            foreach (var queryProvider in _queryProviders)
            {
                query = queryProvider.CreateQuery(this, context, first.Name, (JObject)first.Value);

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
                return new List<string>();
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
}
