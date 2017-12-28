using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Providers
{
    public class ContentItemFieldTypeProvider : IQueryFieldTypeProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITypeActivatorFactory<ContentPart> _typeActivator;
        private readonly IEnumerable<IInputObjectGraphType> _inputGraphTypes;
        private readonly IEnumerable<IGraphQLFilter<ContentItem>> _graphQLFilters;
        private readonly ISession _session;

        public ContentItemFieldTypeProvider(
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            ITypeActivatorFactory<ContentPart> typeActivator,
            IEnumerable<IInputObjectGraphType> inputGraphTypes,
            IEnumerable<IGraphQLFilter<ContentItem>> graphQLFilters,
            ISession session)
        {
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _typeActivator = typeActivator;
            _inputGraphTypes = inputGraphTypes;
            _graphQLFilters = graphQLFilters;
            _session = session;
        }

        public Task<IEnumerable<FieldType>> GetFields(ObjectGraphType state)
        {
            var fieldTypes = new List<FieldType>();

            foreach (var typeDefinition in _contentDefinitionManager.ListTypeDefinitions())
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

                    var queryGraphTypeResolved = (IObjectGraphType)_serviceProvider.GetService(queryGraphType);

                    if (queryGraphTypeResolved != null)
                    {
                        typeType.Field(
                            queryGraphType,
                            partName,
                            resolve: context =>
                            {
                                var nameToResolve = context.ReturnType.Name;
                                var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                                return context.Source.Get(typeToResolve, nameToResolve);
                            });
                    }

                    var inputGraphTypeResolved = _serviceProvider.GetService(inputGraphType) as IQueryArgumentObjectGraphType;

                    if (inputGraphTypeResolved != null)
                    {
                        queryArguments.Add(new QueryArgument(inputGraphTypeResolved)
                        {
                            Name = partName
                        });
                    }
                }

                var query = new ContentItemsQuery(
                    _contentManager, 
                    _graphQLFilters,
                    _session)
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
