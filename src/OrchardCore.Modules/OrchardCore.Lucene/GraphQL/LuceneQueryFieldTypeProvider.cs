using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Lucene;

namespace OrchardCore.Queries.Lucene.GraphQL.Queries
{
    public class LuceneQueryFieldTypeProvider : IQueryFieldTypeProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDependencyResolver _dependencyResolver;

        public LuceneQueryFieldTypeProvider(
            IHttpContextAccessor httpContextAccessor,
            IDependencyResolver dependencyResolver)
        {
            _httpContextAccessor = httpContextAccessor;
            _dependencyResolver = dependencyResolver;
        }

        public async Task<IEnumerable<FieldType>> GetFields(ObjectGraphType state)
        {
            var queryManager = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IQueryManager>();
            var queries = await queryManager.ListQueriesAsync();

            var fieldTypes = new List<FieldType>();

            foreach (var query in queries.OfType<LuceneQuery>())
            {
                if (String.IsNullOrWhiteSpace(query.Schema))
                    continue;

                var name = query.Name;
                var source = query.Source;
                
                var schema = JObject.Parse(query.Schema);

                var type = schema["type"].ToString();

                if (query.ReturnContentItems &&
                    type.StartsWith("ContentItem/", System.StringComparison.OrdinalIgnoreCase))
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

        private FieldType BuildSchemaBasedFieldType(ObjectGraphType state, LuceneQuery query, JToken schema)
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

                Resolver = new AsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("Parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    return await queryManager.ExecuteQueryAsync(iquery, queryParameters);
                }),
                Type = typeof(ListGraphType<ObjectGraphType<JObject>>)
            };

            return fieldType;
        }

        private FieldType BuildContentTypeFieldType(ObjectGraphType state, string contentType, LuceneQuery query)
        {
            var typetype = state.Fields.OfType<ContentItemsQuery>().First(x => x.Name == contentType);

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "Parameters" }
                ),

                Name = query.Name,
                ResolvedType = typetype.ResolvedType,
                Resolver = new AsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("Parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    return await queryManager.ExecuteQueryAsync(iquery, queryParameters);
                }),
                Type = typetype.Type
            };

            return fieldType;
        }
    }
}
