using System;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Contents.Apis.GraphQL.Queries.Types
{
    public class ContentItemType : ObjectGraphType<ContentItem>
    {
        private readonly string ContentPartTypeIndexerName = "ContentPartType";

        public ContentItemType()
        {
            Name = "ContentItem";

            Field(ci => ci.ContentItemId);
            Field(ci => ci.ContentItemVersionId);
            Field(ci => ci.ContentType);
            Field(ci => ci.Number);
            Field(ci => ci.Published);
            Field(ci => ci.Latest);
            Field("ModifiedUtc", ci => ci.ModifiedUtc.Value);
            Field(ci => ci.PublishedUtc, nullable: true);
            Field("CreatedUtc", ci => ci.CreatedUtc.Value);
            Field(ci => ci.Owner);
            Field(ci => ci.Author);

            // TODO: Return content parts?
        }
        
        public bool TryAddContentPart(ContentTypePartDefinition definition, ContentPart contentPart)
        {
            var filterGraphType = new ContentPartAutoRegisteringObjectGraphType(contentPart);

            if (!filterGraphType.Fields.Any())
            {
                return false;
            }

            var field = new FieldType
            {
                Type = filterGraphType.GetType(),
                Name = definition.Name,
                Resolver = new FuncFieldResolver<ContentItem, object> (context => {
                    var contentPartType = context
                    .FieldDefinition
                    .GetMetadata<Type>(ContentPartTypeIndexerName);

                    return context
                        .Source
                        .Get(contentPartType, contentPartType.Name);
                }),
                ResolvedType = filterGraphType
            };

            field.Metadata[ContentPartTypeIndexerName] = contentPart.GetType();

            AddField(field);

            return true;
        }
    }
}
