using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata;
using System.Collections.Concurrent;
using GraphQL.Resolvers;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// Registers all Content Types as queries.
    /// </summary>
    public class ContentTypeQuery : ISchemaBuilder
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentTypeQuery(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var serviceProvider = _httpContextAccessor.HttpContext.RequestServices;

            var contentDefinitionManager = serviceProvider.GetService<IContentDefinitionManager>();
            var partTypeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentPart>>();
            var fieldTypeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentField>>();

            var internalCache = new ConcurrentDictionary<string, IObjectGraphType>();

            foreach (var typeDefinition in contentDefinitionManager.ListTypeDefinitions())
            {
                var typeType = new ContentItemType
                {
                    Name = typeDefinition.Name // Blog
                };

                var queryArguments = new List<QueryArgument>();

                foreach (var part in typeDefinition.Parts)
                {
                    var partName = part.PartDefinition.Name; // BagPart

                    var partActivator = partTypeActivator.GetTypeActivator(partName);

                    var partInputGraphType = typeof(InputObjectGraphType<>).MakeGenericType(partActivator.Type);

                    var partQueryGraphTypeResolved =
                        internalCache.GetOrAdd(partName, (key) =>
                        {
                            var partQueryGraphType = typeof(ObjectGraphType<>).MakeGenericType(partActivator.Type);

                            var resolvedGraphType = (IObjectGraphType)serviceProvider.GetService(partQueryGraphType);

                            if (resolvedGraphType == null) {
                                return null;
                            }

                            foreach (var fieldDefinition in part.PartDefinition.Fields)
                            {
                                var fieldName = fieldDefinition.FieldDefinition.Name;

                                var fieldActivator = fieldTypeActivator.GetTypeActivator(fieldName);

                                var fieldQueryGraphType = typeof(ObjectGraphType<>).MakeGenericType(fieldActivator.Type);

                                var fieldInputGraphType = typeof(InputObjectGraphType<>).MakeGenericType(fieldActivator.Type);

                                var fieldQueryGraphTypeResolved = (IObjectGraphType)serviceProvider.GetService(fieldQueryGraphType);

                                if (fieldQueryGraphTypeResolved == null) {
                                    continue;
                                }

                                resolvedGraphType.AddField(new FieldType {
                                    Name = fieldName,
                                    Type = fieldQueryGraphTypeResolved.GetType(),
                                    Resolver = new AsyncFieldResolver<ContentField>((context) => {

                                        return null;
                                    })
                                });
                            }

                            return resolvedGraphType;
                        });

                    if (partQueryGraphTypeResolved != null)
                    {
                        typeType.Field(
                            partQueryGraphTypeResolved.GetType(),
                            partName,
                            resolve: context =>
                            {
                                var nameToResolve = context.ReturnType.Name;
                                var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                                return context.Source.Get(typeToResolve, nameToResolve);
                            });
                    }

                    var partInputGraphTypeResolved = serviceProvider.GetService(partInputGraphType) as IQueryArgumentObjectGraphType;

                    if (partInputGraphTypeResolved != null)
                    {
                        queryArguments.Add(new QueryArgument(partInputGraphTypeResolved)
                        {
                            Name = partName
                        });
                    }
                }

                var query = new ContentItemsFieldType(_httpContextAccessor)
                {
                    Name = typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType)
                };

                foreach (var qa in queryArguments) {
                    query.Arguments.Add(qa);
                }

                schema.Query.AddField(query);
            }

            return Task.FromResult(contentDefinitionManager.ChangeToken);
        }
    }
}
