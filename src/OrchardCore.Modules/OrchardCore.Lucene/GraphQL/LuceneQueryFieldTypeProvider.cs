using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.Lucene;

namespace OrchardCore.Queries.Lucene.GraphQL.Queries
{
    public class LuceneQueryFieldTypeProvider : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LuceneQueryFieldTypeProvider> _logger;

        public LuceneQueryFieldTypeProvider(IHttpContextAccessor httpContextAccessor, ILogger<LuceneQueryFieldTypeProvider> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public Task<string> GetIdentifierAsync()
        {
            var queryManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IQueryManager>();
            return queryManager.GetIdentifierAsync();
        }

        public async Task BuildAsync(ISchema schema)
        {
            var queryManager = _httpContextAccessor.HttpContext.RequestServices.GetService<IQueryManager>();

            var queries = await queryManager.ListQueriesAsync();

            foreach (var query in queries.OfType<LuceneQuery>())
            {
                if (String.IsNullOrWhiteSpace(query.Schema))
                    continue;

                var name = query.Name;

                try
                {
                    var querySchema = JObject.Parse(query.Schema);
                    if (!querySchema.ContainsKey("type"))
                    {
                        _logger.LogError("The Query '{Name}' schema is invalid, the 'type' property was not found.", name);
                        continue;
                    }
                    var type = querySchema["type"].ToString();
                    FieldType fieldType;

                    if (query.ReturnContentItems &&
                        type.StartsWith("ContentItem/", StringComparison.OrdinalIgnoreCase))
                    {
                        var contentType = type.Remove(0, 12);
                        fieldType = BuildContentTypeFieldType(schema, contentType, query);
                    }
                    else
                    {
                        fieldType = BuildSchemaBasedFieldType(query, querySchema);
                    }

                    if (fieldType != null)
                    {
                        schema.Query.AddField(fieldType);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "The Query '{Name}' has an invalid schema.", name);
                }
            }
        }

        private FieldType BuildSchemaBasedFieldType(LuceneQuery query, JToken querySchema)
        {
            var properties = querySchema["properties"];
            if (properties == null)
            {
                return null;
            }

            var typetype = new ObjectGraphType<JObject>
            {
                Name = query.Name + "Query"
            };

            foreach (JProperty child in properties.Children())
            {
                var name = child.Name;
                var nameLower = name.Replace('.', '_');
                var type = child.Value["type"].ToString();
                var description = child.Value["description"]?.ToString();

                if (type == "string")
                {
                    var field = typetype.Field(
                        typeof(StringGraphType),
                        nameLower,
                        description: description,
                        resolve: context =>
                        {
                            var source = context.Source;
                            return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<string>();
                        });
                    field.Metadata.Add("Name", name);
                }
                else if (type == "integer")
                {
                    var field = typetype.Field(
                        typeof(IntGraphType),
                        nameLower,
                        description: description,
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
                    new QueryArgument<StringGraphType> { Name = "parameters" }
                ),

                Name = query.Name + "Query",
                ResolvedType = new ListGraphType(typetype),
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.ResolveServiceProvider().GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(context.FieldName[0..^5]);

                    var parameters = context.GetArgument<string>("parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    var result = await queryManager.ExecuteQueryAsync(iquery, queryParameters);
                    return result.Items;
                }),
                Type = typeof(ListGraphType<ObjectGraphType<JObject>>)
            };

            return fieldType;
        }

        private FieldType BuildContentTypeFieldType(ISchema schema, string contentType, LuceneQuery query)
        {
            var typetype = schema.Query.Fields.OfType<ContentItemsFieldType>().FirstOrDefault(x => x.Name == contentType);
            if (typetype == null)
            {
                return null;
            }

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                        new QueryArgument<StringGraphType> { Name = "parameters" }
                    ),

                Name = query.Name,
                ResolvedType = typetype.ResolvedType,
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.ResolveServiceProvider().GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(context.FieldName);

                    var parameters = context.GetArgument<string>("parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    var result = await queryManager.ExecuteQueryAsync(iquery, queryParameters);
                    return result.Items;
                }),
                Type = typetype.Type
            };

            return fieldType;
        }
    }
}
