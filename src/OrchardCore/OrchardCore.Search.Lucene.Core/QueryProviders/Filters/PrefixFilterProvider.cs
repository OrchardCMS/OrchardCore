using System;
using System.Linq;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Search.Lucene.QueryProviders.Filters
{
    public class PrefixFilterProvider : ILuceneBooleanFilterProvider
    {
        public FilteredQuery CreateFilteredQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JToken filter, Query toFilter)
        {
            if (type != "prefix")
            {
                return null;
            }

            if (!(toFilter is BooleanQuery booleanQuery))
            {
                return null;
            }

            var queryObj = filter as JObject;
            var first = queryObj.Properties().First();

            // A prefix query has only one member, which can either be a string or an object
            PrefixQuery prefixQuery;

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    prefixQuery = new PrefixQuery(new Term(first.Name, first.Value.ToString()));
                    break;
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (obj.TryGetValue("value", out var value))
                    {
                        prefixQuery = new PrefixQuery(new Term(first.Name, value.Value<string>()));
                    }
                    else if (obj.TryGetValue("prefix", out var prefix))
                    {
                        prefixQuery = new PrefixQuery(new Term(first.Name, prefix.Value<string>()));
                    }
                    else
                    {
                        throw new ArgumentException("Prefix query misses prefix value");
                    }

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        prefixQuery.Boost = boost.Value<float>();
                    }

                    break;
                default: throw new ArgumentException("Invalid prefix query");
            }

            booleanQuery.Add(prefixQuery, Occur.MUST);
            var queryFilter = new QueryWrapperFilter(prefixQuery);

            return new FilteredQuery(booleanQuery, queryFilter);
        }
    }
}
