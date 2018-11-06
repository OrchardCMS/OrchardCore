using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentItemInterface : InterfaceGraphType<ContentItem>
    {
        public ContentItemInterface()
        {
            Name = "ContentItem";

            Field(ci => ci.ContentItemId);
            Field(ci => ci.ContentItemVersionId);
            Field(ci => ci.ContentType);
            Field(ci => ci.DisplayText);
            Field(ci => ci.Published);
            Field(ci => ci.Latest);
            Field<DateTimeGraphType>("modifiedUtc", resolve: ci => ci.Source.ModifiedUtc);
            Field<DateTimeGraphType>("publishedUtc", resolve: ci => ci.Source.PublishedUtc);
            Field<DateTimeGraphType>("createdUtc", resolve: ci => ci.Source.CreatedUtc);
            Field(ci => ci.Owner);
            Field(ci => ci.Author);
        }
    }
}