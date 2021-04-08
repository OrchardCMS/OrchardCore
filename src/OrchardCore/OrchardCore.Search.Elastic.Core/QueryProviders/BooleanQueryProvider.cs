using System;
using Lucene.Net.Search;
using Newtonsoft.Json.Linq;


namespace OrchardCore.Search.Elastic.QueryProviders
{

    /// <summary>
    /// We may not need this at all as Elastic has full blown DSL with NEST OOTB
    /// </summary>
    public class BooleanQueryProvider : IElasticQueryProvider
    {
        public Query CreateQuery(IElasticQueryService builder, ElasticQueryContext context, string type, JObject query)
        {
            if (type != "bool")
            {
                return null;
            }

            var boolQuery = new BooleanQuery();

            foreach (var property in query.Properties())
            {
                var occur = Occur.MUST;
                bool isProps = false;

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
                        boolQuery.Boost = property.Value.Value<float>();
                        isProps = true;
                        break;
                    case "minimum_should_match":
                        boolQuery.MinimumNumberShouldMatch = property.Value.Value<int>();
                        isProps = true;
                        break;
                    default: throw new ArgumentException($"Invalid property '{property.Name}' in boolean query");
                }

                if (!isProps)
                {
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
            }

            return boolQuery;
        }
    }
}
