using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders
{
    public class RegexpQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "regexp")
            {
                return null;
            }

            var first = query.Properties().First();

            switch (first.Value.Type)
            {
                case JTokenType.String:
                    return new RegexpQuery(new Term(first.Name, first.Value.ToString()));
                case JTokenType.Object:
                    var obj = (JObject)first.Value;

                    if (!obj.TryGetValue("value", out var value))
                    {
                        throw new ArgumentException("Missing value in regexp query");
                    }

                    // TODO: Support flags

                    var regexpQuery = new RegexpQuery(new Term(value.Value<string>()));

                    if (obj.TryGetValue("boost", out var boost))
                    {
                        regexpQuery.Boost = boost.Value<float>();
                    }

                    return regexpQuery;
                default: throw new ArgumentException("Invalid regexp query");
            }
        }
    }
}
