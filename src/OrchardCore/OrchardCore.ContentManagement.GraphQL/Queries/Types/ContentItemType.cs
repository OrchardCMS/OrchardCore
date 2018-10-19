using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentItemType : ObjectGraphType<ContentItem>
    {
        public ContentItemType()
        {
            Name = "ContentItemType";

            Field(ci => ci.ContentItemId);
            Field(ci => ci.ContentItemVersionId);
            Field(ci => ci.ContentType);
            Field(ci => ci.Published);
            Field(ci => ci.Latest);
            Field("modifiedUtc", ci => ci.ModifiedUtc.Value);
            Field(ci => ci.PublishedUtc, nullable: true);
            Field("createdUtc", ci => ci.CreatedUtc.Value);
            Field(ci => ci.Owner);
            Field(ci => ci.Author);

            Interface<ContentItemInterface>();

            IsTypeOf = IsContentType;
        }

        private bool IsContentType(object obj)
        {
            return obj is ContentItem;
        }
    }
}
