using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene
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
            var queryProp = queryObj["query"] as JObject
                ?? throw new ArgumentException("Query DSL requires a [query] property");

            var query = CreateQueryFragment(context, queryProp);

            var sortProperty = queryObj["sort"];
            var fromProperty = queryObj["from"];
            var sizeProperty = queryObj["size"];

            var size = sizeProperty?.Value<int>() ?? 10;
            var from = fromProperty?.Value<int>() ?? 0;

            string sortField = null;
            string sortOrder = null;

            var sortFields = new List<SortField>();

            if (sortProperty != null)
            {
                string sortType;

                if (sortProperty.Type == JTokenType.String)
                {
                    sortField = sortProperty.ToString();
                    sortFields.Add(new SortField(sortField, SortFieldType.STRING, sortOrder == "desc"));
                }
                else if (sortProperty.Type == JTokenType.Object)
                {
                    sortField = ((JProperty)sortProperty.First).Name;
                    sortOrder = ((JProperty)sortProperty.First).Value["order"].ToString();
                    sortType = ((JProperty)sortProperty.First).Value["type"]?.ToString();
                    var sortFieldType = SortFieldType.STRING;

                    if (sortType != null)
                    {
                        sortFieldType = (SortFieldType)Enum.Parse(typeof(SortFieldType), sortType.ToUpper());
                    }

                    sortFields.Add(new SortField(sortField, sortFieldType, sortOrder == "desc"));
                }
                else if (sortProperty.Type == JTokenType.Array)
                {
                    foreach (var item in sortProperty.Children())
                    {
                        sortField = ((JProperty)item.First).Name;
                        sortOrder = ((JProperty)item.First).Value["order"].ToString();
                        sortType = ((JProperty)item.First).Value["type"]?.ToString();
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
            if (String.IsNullOrEmpty(text))
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
