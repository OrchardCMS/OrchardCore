using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Mutations.Types
{
    public class CreateContentItemInputType : InputObjectGraphType<ContentItem>
    {
        public CreateContentItemInputType()
        {
            Field(ci => ci.ContentType, false);
            Field(ci => ci.Author, true);
            Field(ci => ci.Owner, true);

            Field(ci => ci.Published, true);
            Field(ci => ci.Latest, true);

            Field<ContentPartsInputType>("ContentParts");
        }
    }

    public class ContentPartsInputType : InputObjectGraphType
    {
        public ContentPartsInputType(
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<ContentPart> _contentParts)
        {
            Name = "ContentPartsInput";

            foreach (var contentPartDefinition in contentDefinitionManager.ListPartDefinitions())
            {
                var contentPart = _contentParts.FirstOrDefault(x => x.GetType().Name == contentPartDefinition.Name);

                if (contentPart != null)
                {
                    var filterGraphType = new InputContentPartAutoRegisteringObjectGraphType(contentPart);

                    if (filterGraphType.Fields.Any())
                    {
                        var name = contentPart.GetType().Name; // About
                        var partName = contentPartDefinition.Name; // BagPart

                        // Add Field needs to be like Content Item and Content Items.... 
                        // so you can filter by blog...
                        var field = new FieldType
                        {
                            Type = filterGraphType.GetType(),
                            Name = name,
                            Resolver = new FuncFieldResolver<object>(context =>
                            {
                                if (context.Source == null)
                                {
                                    return null;
                                }

                                var contentPartType = (Type)context.FieldDefinition.Metadata["contentPartType"];

                                return ((ContentItem)context.Source).Get(contentPartType, contentPartType.Name);
                            }),
                            ResolvedType = filterGraphType,
                        };

                        field.Metadata.Add("contentPartType", contentPart.GetType());

                        AddField(field);
                    }
                }
            }
        }
    }
}
