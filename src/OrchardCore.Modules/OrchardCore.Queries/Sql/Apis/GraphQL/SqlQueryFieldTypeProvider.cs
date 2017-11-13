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

namespace OrchardCore.Queries.Sql.Apis.GraphQL.Queries
{
    public class SqlQueryFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IQueryManager _queryManager;
        private readonly IEnumerable<QueryFieldType> _queryFieldTypes;
        private readonly IDependencyResolver _dependencyResolver;

        public SqlQueryFieldTypeProvider(IQueryManager queryManager,
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

            foreach (var query in queries.OfType<SqlQuery>())
            {
                var name = query.Name;
                var source = query.Source;

                if (!query.ReturnDocuments)
                {
                    if (!string.IsNullOrWhiteSpace(query.Schema))
                    {
                        fieldTypes.Add(BuildSchemaBasedFieldType(state, query));
                    }
                }
                else
                {
                    fieldTypes.Add(BuildContentTypeFieldType(state, query));
                }
            }

            return fieldTypes;
        }

        private FieldType BuildSchemaBasedFieldType(ObjectGraphType state, SqlQuery query)
        {
            var schemaJson = JObject.Parse(query.Schema);

            var schema = schemaJson["schema"];

            var typetype = new ObjectGraphType<JObject>
            {
                Name = query.Name
            };

            foreach (var child in schema.Children().OfType<JProperty>())
            {
                var name = child.Name.ToString().Replace('.', '_');
                var value = child.Value.ToString();

                if (value == "String")
                {
                    var field = typetype.Field(
                        typeof(StringGraphType),
                        name,
                        resolve: context =>
                        {
                            var source = context.Source;
                            return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<string>();
                        });
                    field.Metadata.Add("Name", child.Name.ToString());
                }
                if (value == "Integer")
                {
                    var field = typetype.Field(
                        typeof(IntGraphType),
                        name,
                        resolve: context =>
                        {
                            var source = context.Source;
                            return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<int>();
                        });
                    field.Metadata.Add("Name", child.Name.ToString());
                }
            }

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "Parameters" }
                ),

                Name = query.Name,
                ResolvedType = new ListGraphType(typetype),
                Resolver = new SlowFuncFieldResolver<object, Task<object>>(async context => {
                    var iquery = await _queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("Parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    var p = await _queryManager.ExecuteQueryAsync(iquery, queryParameters);
                    return p;
                }),
                Type = typeof(ListGraphType<ObjectGraphType<JObject>>)
            };

            return fieldType;
        }

        private FieldType BuildContentTypeFieldType(ObjectGraphType state, SqlQuery query)
        {
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
