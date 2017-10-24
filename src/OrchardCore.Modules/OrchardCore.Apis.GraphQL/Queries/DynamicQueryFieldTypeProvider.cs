using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.Apis.GraphQL.Queries
{
    public class DynamicQueryFieldTypeProvider : IDynamicQueryFieldTypeProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<ContentPart> _contentParts;
        private readonly IEnumerable<IObjectGraphType> _objectGraphTypes;
        private readonly ISession _session;

        public DynamicQueryFieldTypeProvider(
         IServiceProvider serviceProvider,
         IContentManager contentManager,
         IContentDefinitionManager contentDefinitionManager,
         IEnumerable<ContentPart> contentParts,
         IEnumerable<IObjectGraphType> objectGraphTypes,
         ISession session)
        {
            _serviceProvider = serviceProvider;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentParts = contentParts;
            _objectGraphTypes = objectGraphTypes;
            _session = session;
        }

        public IEnumerable<FieldType> GetFields()
        {
            var fieldTypes = new List<FieldType>();

            var typeDefinitions = _contentDefinitionManager.ListTypeDefinitions();

            foreach (var typeDefinition in typeDefinitions)
            {
                var typeType = new ContentItemType
                {
                    Name = typeDefinition.Name, // Blog
                };

                foreach (var part in typeDefinition.Parts)
                {
                    var name = part.Name; // About
                    var partName = part.PartDefinition.Name; // BagPart

                    var contentPart = _contentParts.FirstOrDefault(x => x.GetType().Name == partName);

                    if (contentPart != null)
                    {
                        var p = new ContentPartAutoRegisteringObjectGraphType(contentPart);

                        //var p = _objectGraphTypes.FirstOrDefault(x => x.IsTypeOf(contentPart));

                        if (p != null)
                        {
                            // Add Field needs to be like Content Item and Content Items.... 
                            // so you can filter by blog...
                            var fieldType = new FieldType
                            {
                                Name = name,
                                ResolvedType = p,
                                Resolver = new FuncFieldResolver<object>(context => {
                                    var contentPartType = (Type)context.FieldDefinition.Metadata["contentPartType"];

                                    return ((ContentItem)context.Source).Get(contentPartType, contentPartType.Name);
                                }),
                                Type = p.GetType(),
                            };

                            fieldType.Metadata.Add("contentPartType", contentPart.GetType());

                            typeType.AddField(fieldType);
                        }
                    }
                }


                fieldTypes.Add(new ContentItemsQuery(_contentManager, _session)
                {
                    Name = typeDefinition.Name,
                    ResolvedType = new ListGraphType(typeType)
                });
            }

            return fieldTypes;
        }
    }
}
