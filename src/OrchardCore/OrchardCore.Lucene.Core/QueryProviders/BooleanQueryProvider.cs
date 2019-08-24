using System;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Lucene.QueryProviders
{
    public class BooleanQueryProvider : ILuceneQueryProvider
    {
        public Query CreateQuery(ILuceneQueryService builder, LuceneQueryContext context, string type, JObject query)
        {
            if (type != "bool")
            {
                return null;
            }

            var boolQuery = new BooleanQuery();

            foreach (var property in query.Properties())
            {
                var occur = Occur.MUST;

                switch (property.Name.ToLowerInvariant())
                {
                    case "must":
                        occur = Occur.MUST;
                        break;
                    case "mustnot":
                    case "must_not":
                        occur = Occur.MUST_NOT;
                        break;
                    case "should":
                        occur = Occur.SHOULD;
                        break;
                    case "boost":
                        boolQuery.Boost = query.Value<float>();
                        break;
                    case "minimum_should_match":
                        boolQuery.MinimumNumberShouldMatch = query.Value<int>();
                        break;
                    default: throw new ArgumentException($"Invalid property '{property.Name}' in boolean query");
                }

                switch (property.Value.Type)
                {
                    case JTokenType.Object:

                        boolQuery.Add(builder.CreateQueryFragment(context, (JObject)property.Value), occur);

                        break;
                    case JTokenType.Array:
                        foreach (var item in ((JArray)property.Value))
                        {
                            if (item.Type != JTokenType.Object)
                            {
                                throw new ArgumentException($"Invalid value in boolean query");
                            }
                            boolQuery.Add(builder.CreateQueryFragment(context, (JObject)item), occur);
                        }
                        break;
                    default: throw new ArgumentException($"Invalid value in boolean query");

                }
            }

            return boolQuery;
        }
    }
}
