using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene
{
    public class LuceneQueryService : ILuceneQueryService
    {
        private readonly IEnumerable<ILuceneQueryProvider> _queryProviders;

        public LuceneQueryService(IEnumerable<ILuceneQueryProvider> queryProviders)
        {
            _queryProviders = queryProviders;
        }

        public Task<TopDocs> SearchAsync(LuceneQueryContext context, JObject queryObj)
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

            string sortField = null;
            string sortOrder = null;

            if (sortProperty != null)
            {
                if (sortProperty.Type == JTokenType.String)
                {
                    sortField = sortProperty.ToString();
                }
                else if (sortProperty.Type == JTokenType.Object)
                {
                    sortField = ((JProperty)sortProperty.First).Name;
                    sortOrder = ((JProperty)sortProperty.First).Value["order"].ToString();
                }
            }

            TopDocs docs = context.IndexSearcher.Search(
                query,
                size + from,
                sortField == null ? Sort.RELEVANCE : new Sort(new SortField(sortField, SortFieldType.STRING, sortOrder == "desc"))
            );

            if (from > 0)
            {
                docs = new TopDocs(docs.TotalHits - from, docs.ScoreDocs.Skip(from).ToArray(), docs.MaxScore);
            }

            return Task.FromResult(docs);
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
