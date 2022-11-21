using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement.GraphQL.Queries;

namespace OrchardCore.Queries.Sql.GraphQL.Queries
{
    /// <summary>
    /// This implementation of <see cref="ISchemaBuilder"/> registers
    /// all SQL Queries as GraphQL queries.
    /// </summary>
    public class SqlQueryFieldTypeProvider : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SqlQueryFieldTypeProvider> _logger;
        private readonly GraphQLSettings _graphQLSettings;

        public SqlQueryFieldTypeProvider(
            IHttpContextAccessor httpContextAccessor,
            ILogger<SqlQueryFieldTypeProvider> logger,
            IOptions<GraphQLSettings> settingsAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _graphQLSettings = settingsAccessor.Value;
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

            foreach (var query in queries.OfType<SqlQuery>())
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

                    var fieldTypeName = querySchema["fieldTypeName"]?.ToString() ?? query.Name;

                    if (type.StartsWith("ContentItem/", StringComparison.OrdinalIgnoreCase))
                    {
                        var contentType = type.Remove(0, 12);
                        fieldType = BuildContentTypeFieldType(schema, contentType, query, fieldTypeName);
                    }
                    else
                    {
                        fieldType = BuildSchemaBasedFieldType(query, querySchema, fieldTypeName);
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

        private FieldType BuildSchemaBasedFieldType(SqlQuery query, JToken querySchema, string fieldTypeName)
        {
            var properties = querySchema["properties"];
            if (properties == null)
            {
                return null;
            }

            var typetype = new ObjectGraphType<JObject>
            {
                Name = fieldTypeName
            };

            foreach (JProperty child in properties.Children())
            {
                var name = child.Name;
                var nameLower = name.Replace('.', '_');
                var type = child.Value["type"]?.ToString()?.ToLower() ?? "string";

                FieldType field = null;

                switch (type)
                {
                    case "string":
                        field = typetype.Field(
                            typeof(StringGraphType),
                            nameLower,
                            description: child.Value["description"]?.ToString(),
                            resolve: context =>
                            {
                                var source = context.Source;
                                return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<string>();
                            });
                        break;

                    case "integer":
                    case "int":
                        field = typetype.Field(
                           typeof(IntGraphType),
                           nameLower,
                           description: child.Value["description"]?.ToString(),
                           resolve: context =>
                           {
                               var source = context.Source;
                               return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<int>();
                           });
                        break;
                    case "boolean":
                    case "bool":
                        field = typetype.Field(
                           typeof(BooleanGraphType),
                           nameLower,
                           description: child.Value["description"]?.ToString(),
                           resolve: context =>
                           {
                               var source = context.Source;
                               return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<bool>();
                           });
                        break;

                    case "identifier":
                    case "id":
                        field = typetype.Field(
                           typeof(IdGraphType),
                           nameLower,
                           description: child.Value["description"]?.ToString(),
                           resolve: context =>
                           {
                               var source = context.Source;
                               return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<string>();
                           });
                        break;

                    case "double":
                    case "numeric":
                    case "decimal":
                        field = typetype.Field(
                           typeof(FloatGraphType),
                           nameLower,
                           description: child.Value["description"]?.ToString(),
                           resolve: context =>
                           {
                               var source = context.Source;
                               return source[context.FieldDefinition.Metadata["Name"].ToString()].ToObject<decimal>();
                           });
                        break;

                    default:
                        break;
                }

                if (field == null)
                {
                    continue;
                }

                field.Metadata.Add("Name", name);

                if (Boolean.TryParse(child.Value["filterable"]?.ToString(), out var filterable))
                {
                    field.Metadata.Add("Filterable", filterable);
                }

                if (Boolean.TryParse(child.Value["sortable"]?.ToString(), out var sortable))
                {
                    field.Metadata.Add("Sortable", sortable);
                }
            }

            var whereInput = new DynamicWhereInput(typetype.Fields);
            var orderByInput = new DynamicOrderByInput(typetype.Fields);
            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "parameters" },
                    new QueryArgument<DynamicWhereInput> { Name = "where", Description = "filters the content items", ResolvedType = whereInput },
                    new QueryArgument<DynamicOrderByInput> { Name = "orderBy", Description = "sort order", ResolvedType = orderByInput },
                    new QueryArgument<IntGraphType> { Name = "first", Description = "the first n content items", ResolvedType = new IntGraphType() },
                    new QueryArgument<IntGraphType> { Name = "skip", Description = "the number of content items to skip", ResolvedType = new IntGraphType() }
                ),

                Name = fieldTypeName,
                Description = "Represents the " + query.Source + " Query : " + query.Name,
                ResolvedType = new ListGraphType(typetype),
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.RequestServices.GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(query.Name);

                    var first = context.GetArgument<int>("first");

                    if (first == 0)
                    {
                        first = _graphQLSettings.DefaultNumberOfResults;
                    }

                    // Apply Take(first)

                    if (context.HasPopulatedArgument("skip"))
                    {
                        var skip = context.GetArgument<int>("skip");

                        // Apply Skip(skip)
                    }

                    var parameters = context.GetArgument<string>("parameters");

                    var where = context.GetArgument<string>("where");
                    // apply there where logic...

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

        private static FieldType BuildContentTypeFieldType(ISchema schema, string contentType, SqlQuery query, string fieldTypeName)
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

                Name = fieldTypeName,
                Description = "Represents the " + query.Source + " Query : " + query.Name,
                ResolvedType = typetype.ResolvedType,
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.RequestServices.GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(query.Name);

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
