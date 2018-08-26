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
            var typeActivator = serviceProvider.GetService<ITypeActivatorFactory<ContentPart>>();


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

                    var activator = typeActivator.GetTypeActivator(partName);

                    var queryGraphType = typeof(ObjectGraphType<>).MakeGenericType(activator.Type);

                    var inputGraphType = typeof(InputObjectGraphType<>).MakeGenericType(activator.Type);

                    var queryGraphTypeResolved = (IObjectGraphType)serviceProvider.GetService(queryGraphType);

                    if (queryGraphTypeResolved != null)
                    {
                        typeType.Field(
                            queryGraphTypeResolved.GetType(),
                            partName,
                            resolve: context =>
                            {
                                var nameToResolve = context.ReturnType.Name;
                                var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                                return context.Source.Get(typeToResolve, nameToResolve);
                            });
                    }

                    var inputGraphTypeResolved = serviceProvider.GetService(inputGraphType) as IQueryArgumentObjectGraphType;

                    if (inputGraphTypeResolved != null)
                    {
                        queryArguments.Add(new QueryArgument(inputGraphTypeResolved)
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
