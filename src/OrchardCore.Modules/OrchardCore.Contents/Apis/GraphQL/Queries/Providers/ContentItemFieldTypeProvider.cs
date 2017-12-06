using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;
using YesSql;

namespace OrchardCore.Contents.Apis.GraphQL.Queries.Providers
{
    public class ContentItemFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<ContentPart> _contentParts;
        private readonly IEnumerable<IInputObjectGraphType> _inputGraphTypes;
        private readonly ISession _session;

        public ContentItemFieldTypeProvider(
            IServiceProvider serviceProvider,
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<ContentPart> contentParts,
            IEnumerable<IInputObjectGraphType> inputGraphTypes,
            ISession session)
        {
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentParts = contentParts;
            _inputGraphTypes = inputGraphTypes;
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
                    var name = part.Name; // About
                    var partName = part.PartDefinition.Name; // BagPart
                    
                    var contentPart = _contentParts.FirstOrDefault(x => x.GetType().Name == partName);

                    if (contentPart != null)
                    {
                        var queryGraphType =
                            typeof(ObjectGraphType<>).MakeGenericType(contentPart.GetType());

                        var inputGraphType =
                            typeof(InputObjectGraphType<>).MakeGenericType(contentPart.GetType());

                        var queryGraphTypeResolved = (IObjectGraphType)_serviceProvider.GetService(queryGraphType);

                        if (queryGraphTypeResolved != null)
                        {
                            typeType.Field(
                                queryGraphType,
                                partName,
                                resolve: context => {
                                    var nameToResolve = context.ReturnType.Name;
                                    var typeToResolve = context.ReturnType.GetType().BaseType.GetGenericArguments().First();

                                    return context.Source.Get(typeToResolve, nameToResolve);
                                });
                        }

                        var inputGraphTypeResolved = (IInputObjectGraphType)_serviceProvider.GetService(inputGraphType);

                        if (inputGraphTypeResolved != null)
                        {
                            queryArguments.Add(new QueryArgument(inputGraphType)
                            {
                                Name = name,
                                ResolvedType = inputGraphTypeResolved
                            });
                        }
                    }
                }

                var query = new ContentItemsQuery(_contentManager, _contentParts, _session)
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
