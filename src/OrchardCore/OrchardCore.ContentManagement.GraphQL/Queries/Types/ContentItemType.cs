using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentItemType : ObjectGraphType<ContentItem>
    {
        public ContentItemType()
        {
            Name = "ContentItem";

            Field(ci => ci.ContentItemId);
            Field(ci => ci.ContentItemVersionId);
            Field(ci => ci.ContentType);
            Field(ci => ci.Published);
            Field(ci => ci.Latest);
            Field("ModifiedUtc", ci => ci.ModifiedUtc.Value);
            Field(ci => ci.PublishedUtc, nullable: true);
            Field("CreatedUtc", ci => ci.CreatedUtc.Value);
            Field(ci => ci.Owner);
            Field(ci => ci.Author);

            // TODO: Return content parts?
        }
    }
}
