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
using OrchardCore.Lucene.Model;


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

                    var fieldTypeName = querySchema["fieldTypeName"]?.ToString() ?? query.Name;
                    if (querySchema.ContainsKey("hasTotal") && querySchema["hasTotal"].ToString().Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        if (query.ReturnContentItems && type.StartsWith("ContentItem/", StringComparison.OrdinalIgnoreCase))
                        {
                            var contentType = type.Remove(0, 12);
                            fieldType = BuildTotalContentTypeFieldType(schema, contentType, query, fieldTypeName);
                        }
                        else
                        {
                            fieldType = BuildTotalSchemaBasedFieldType(query, querySchema, fieldTypeName);
                        }
                    }
                    else
                    {
                        if (query.ReturnContentItems && type.StartsWith("ContentItem/", StringComparison.OrdinalIgnoreCase))
                        {
                            var contentType = type.Remove(0, 12);
                            fieldType = BuildContentTypeFieldType(schema, contentType, query, fieldTypeName);
                        }
                        else
                        {
                            fieldType = BuildSchemaBasedFieldType(query, querySchema, fieldTypeName);
                        }
                    }

                    if (fieldType != null && !schema.Query.HasField(fieldType.Name))
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

        private FieldType BuildSchemaBasedFieldType(LuceneQuery query, JToken querySchema, string fieldTypeName)
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

                Name = fieldTypeName,
                Description = "Represents the " + query.Source + " Query : " + query.Name,
                ResolvedType = new ListGraphType(typetype),
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.ResolveServiceProvider().GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(query.Name);

                    var parameters = context.GetArgument<string>("parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    var result = (await queryManager.ExecuteQueryAsync(iquery, queryParameters)) as OrchardCore.Lucene.LuceneQueryResults;
                    return result.Items;
                }),
                Type = typeof(ListGraphType<ObjectGraphType<JObject>>)
            };

            return fieldType;
        }

        private FieldType BuildContentTypeFieldType(ISchema schema, string contentType, LuceneQuery query, string fieldTypeName)
        {
            var typetype = schema.Query.Fields.OfType<ContentItemsFieldType>().FirstOrDefault(x => x.Name == contentType);
            if (typetype == null)
            {
                return null;
            }

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                        new QueryArgument<StringGraphType> { Name = "parameters" }),

                Name = fieldTypeName,
                Description = "Represents the " + query.Source + " Query : " + query.Name,
                ResolvedType = typetype.ResolvedType,
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.ResolveServiceProvider().GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(query.Name);

                    var parameters = context.GetArgument<string>("parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();
                    var result = (await queryManager.ExecuteQueryAsync(iquery, queryParameters)) as OrchardCore.Lucene.LuceneQueryResults;
                    return result.Items;
                }),
                Type = typetype.Type
            };

            return fieldType;
        }
        private FieldType BuildTotalSchemaBasedFieldType(LuceneQuery query, JToken querySchema, string fieldTypeName)
        {
            var properties = querySchema["properties"];
            if (properties == null)
            {
                return null;
            }
            var totalType = new ObjectGraphType<OrchardCore.Lucene.LuceneQueryResults>()
            {
                Name = fieldTypeName
            };

            var typetype = new ObjectGraphType<JObject>
            {
                Name = fieldTypeName
            };
            var listType = new ListGraphType(typetype);

            totalType.Field(listType.GetType(), "items",
                resolve: context =>
                {
                    return context.Source?.Items;
                });
            var total = totalType.Field<IntGraphType>("total",
                         resolve: context =>
                         {
                             return context.Source?.Count;
                         });


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

                Name = fieldTypeName,
                Description = "Represents the " + query.Source + " Query : " + query.Name,
                ResolvedType = totalType,
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.ResolveServiceProvider().GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(query.Name);

                    var parameters = context.GetArgument<string>("parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();

                    var result = (await queryManager.ExecuteQueryAsync(iquery, queryParameters)) as OrchardCore.Lucene.LuceneQueryResults;
                    return result;
                }),
                Type = totalType.GetType()
            };

            return fieldType;
        }


        private FieldType BuildTotalContentTypeFieldType(ISchema schema, string contentType, LuceneQuery query, string fieldTypeName)
        {
            var typetype = schema.Query.Fields.OfType<ContentItemsFieldType>().FirstOrDefault(x => x.Name == contentType);
            if (typetype == null)
            {
                return null;
            }

            var totalType = new ObjectGraphType<OrchardCore.Lucene.LuceneQueryResults>
            {
                Name = fieldTypeName
            };

            var items = totalType.Field(typetype.Type, "items",
                         resolve: context =>
                         {
                             return context.Source?.Items ?? Array.Empty<object>();
                         });
            items.ResolvedType = typetype.ResolvedType;
            totalType.Field<IntGraphType>("total",
                        resolve: context =>
                        {
                            return context.Source?.Count ?? 0;
                        });

            var fieldType = new FieldType
            {
                Arguments = new QueryArguments(
                        new QueryArgument<StringGraphType> { Name = "parameters" }),

                Name = fieldTypeName,
                Description = "Represents the " + query.Source + " Query : " + query.Name,
                ResolvedType = totalType,
                Resolver = new LockedAsyncFieldResolver<object, object>(async context =>
                {
                    var queryManager = context.ResolveServiceProvider().GetService<IQueryManager>();
                    var iquery = await queryManager.GetQueryAsync(query.Name);

                    var parameters = context.GetArgument<string>("parameters");

                    var queryParameters = parameters != null ?
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters)
                        : new Dictionary<string, object>();
                    var result = await queryManager.ExecuteQueryAsync(iquery, queryParameters);

                    return result as OrchardCore.Lucene.LuceneQueryResults;
                }),
                Type = totalType.GetType()
            };

            return fieldType;
        }


    }

}
