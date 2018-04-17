using System;
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
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.Queries.Sql.GraphQL.Queries
{
    public class SqlQueryFieldTypeProvider : IQueryFieldTypeProvider
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
                if (String.IsNullOrWhiteSpace(query.Schema))
                    continue;

                var name = query.Name;
                var source = query.Source;

                var schema = JObject.Parse(query.Schema);

                var type = schema["type"].ToString();

                if (type.StartsWith("ContentItem/", System.StringComparison.OrdinalIgnoreCase))
                {
                    var contentType = type.Remove(0, 12);
                    fieldTypes.Add(BuildContentTypeFieldType(state, contentType, query));
                }
                else
                {
                    fieldTypes.Add(BuildSchemaBasedFieldType(state, query, schema));
                }
            }

            return fieldTypes;
        }

        private FieldType BuildSchemaBasedFieldType(ObjectGraphType state, SqlQuery query, JToken schema)
        {
            var typetype = new ObjectGraphType<JObject>
            {
                Name = query.Name
            };

            var properties = schema["Properties"];

            foreach (var child in properties.Children())
            {
                var name = ((JProperty)child).Name;
                var nameLower = name.Replace('.', '_');
                var type = child["type"].ToString();

                if (type == "String")
                {
                    var field = typetype.Field(
                        typeof(StringGraphType),
                        nameLower,
                        resolve: context =>
                        {
                            var source = context.Source;
                            return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<string>();
                        });
                    field.Metadata.Add("Name", name);
                }
                if (type == "Integer")
                {
                    var field = typetype.Field(
                        typeof(IntGraphType),
                        nameLower,
                        resolve: context =>
                        {
                            var source = context.Source;
                            return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<int>();
                        });
                    field.Metadata.Add("Name", name);
                }
            }

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "Parameters" }
                ),

                Name = query.Name,
                ResolvedType = new ListGraphType(typetype),
                Resolver = new AsyncFieldResolver<object, object>(async context => {
                    var iquery = await _queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("Parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    return await _queryManager.ExecuteQueryAsync(iquery, queryParameters);
                }),
                Type = typeof(ListGraphType<ObjectGraphType<JObject>>)
            };

            return fieldType;
        }

        private FieldType BuildContentTypeFieldType(ObjectGraphType state, string contentType, SqlQuery query)
        {
            var typetype = state.Fields.OfType<ContentItemsQuery>().First(x => x.Name == contentType);

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "Parameters" }
                ),

                Name = query.Name,
                ResolvedType = typetype.ResolvedType,
                Resolver = new AsyncFieldResolver<object, object>(async context => {
                    var iquery = await _queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("Parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    return await _queryManager.ExecuteQueryAsync(iquery, queryParameters);
                }),
                Type = typetype.Type
            };

            return fieldType;
        }
    }
}
