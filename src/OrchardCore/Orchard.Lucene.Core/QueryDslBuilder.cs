using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace Orchard.Lucene
{
    public class QueryDslBuilder : IQueryDslBuilder
    {
        private readonly IEnumerable<ILuceneQueryProvider> _queryProviders;

        public QueryDslBuilder(IEnumerable<ILuceneQueryProvider> queryProviders)
        {
            _queryProviders = queryProviders;
        }

        public Query CreateQuery(LuceneQueryContext context, JObject queryObj)
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

            if (query == null)
            {
                throw new ArgumentException("Invalid query");
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
            using (var tokenStream = analyzer.TokenStream(fieldName, text))
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
