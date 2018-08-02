using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Providers
{
    public class ContentItemFieldTypeProvider : IQueryFieldTypeProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITypeActivatorFactory<ContentPart> _typeActivator;

        public ContentItemFieldTypeProvider(
            IHttpContextAccessor httpContextAccessor,
            ITypeActivatorFactory<ContentPart> typeActivator)
        {
            _httpContextAccessor = httpContextAccessor;
            _typeActivator = typeActivator;
        }

        public Task<IEnumerable<FieldType>> GetFields(ObjectGraphType state)
        {
            var fieldTypes = new List<FieldType>();

            var contentDefinitionManager = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentDefinitionManager>();

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

                    var activator = _typeActivator.GetTypeActivator(partName);

                    var queryGraphType =
                        typeof(ObjectGraphType<>).MakeGenericType(activator.Type);

                    var inputGraphType =
                        typeof(InputObjectGraphType<>).MakeGenericType(activator.Type);

                    var queryGraphTypeResolved = (IObjectGraphType)_httpContextAccessor.HttpContext.RequestServices.GetService(queryGraphType);

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

                    var inputGraphTypeResolved = _httpContextAccessor.HttpContext.RequestServices.GetService(inputGraphType) as IQueryArgumentObjectGraphType;

                    if (inputGraphTypeResolved != null)
                    {
                        queryArguments.Add(new QueryArgument(inputGraphTypeResolved)
                        {
                            Name = partName
                        });
                    }
                }

                var query = new ContentItemsQuery(_httpContextAccessor)
                {
                    Name = typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType)
                };

                query.Arguments.AddRange(queryArguments);

                fieldTypes.Add(query);
            }

            return Task.FromResult<IEnumerable<FieldType>>(fieldTypes);
        }
    }
}
