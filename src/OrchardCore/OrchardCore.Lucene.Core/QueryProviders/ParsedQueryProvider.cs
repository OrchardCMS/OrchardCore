using System;
using System.Linq;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders
{
    public class ParsedQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "parsed")
            {
                return null;
            }

            var first = query.Properties().First();

            Query CreateQuery(string searchQuery)
            {
                var queryParser = new MultiFieldQueryParser(context.DefaultVersion, new[] { first.Name }, context.DefaultAnalyzer);
                return queryParser.Parse(searchQuery);
            }

            // A parsed query has only one member, which can either be a string or an object

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return CreateQuery(first.Value.ToString());
                case JTokenType.Object:
                    var obj = (JObject)first.Value;
                    var searchQuery = obj.Property("query").Value.Value<string>();
                    var parsedQuery = CreateQuery(searchQuery);

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        parsedQuery.Boost = boost.Value<float>();
                    }

                    return parsedQuery;
                default: throw new ArgumentException("Invalid term query");
            }
        }
    }
}
