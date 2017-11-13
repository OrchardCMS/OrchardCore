using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.Contents.Apis.GraphQL.Queries;
using OrchardCore.Lucene;

namespace OrchardCore.Queries.Lucene.Apis.GraphQL.Queries
{
    public class LuceneQueryFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<QueryFieldType> _queryFieldTypes;
        private readonly IDependencyResolver _dependencyResolver;

        public LuceneQueryFieldTypeProvider(IQueryManager queryManager,
            IEnumerable<QueryFieldType> queryFieldTypes,
            IDependencyResolver dependencyResolver)
        {
            _queryManager = queryManager;
            _queryFieldTypes = queryFieldTypes;
            _dependencyResolver = dependencyResolver;
        }

        public async Task<IEnumerable<FieldType>> GetFields(ObjectGraphType state)
        {
            var queries = await _queryManager.ListQueriesAsync();

            var fieldTypes = new List<FieldType>();

            foreach (var query in queries.OfType<LuceneQuery>())
            {
                var name = query.Name;
                var source = query.Source;

                if (!query.ReturnContentItems)
                {
                    continue;
                }
                else
                {
                    fieldTypes.Add(BuildContentTypeFieldType(state, query));
                }
            }

            return fieldTypes;
        }

        private FieldType BuildContentTypeFieldType(ObjectGraphType state, LuceneQuery query) {

            var queryvalue = JObject.Parse(query.Template)["query"]["term"]["Content.ContentItem.ContentType"].ToObject<string>();

            var typetype = state.Fields.OfType<ContentItemsQuery>().First(x => x.Name == queryvalue);

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "Parameters" }
                ),

                Name = query.Name,
                ResolvedType = typetype.ResolvedType,
                Resolver = new SlowFuncFieldResolver<object, Task<object>>(async context => {
                    var iquery = await _queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("Parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    var p = await _queryManager.ExecuteQueryAsync(iquery, queryParameters);
                    return p;
                }),
                Type = typetype.Type
            };

            return fieldType;
        }
    }
}
