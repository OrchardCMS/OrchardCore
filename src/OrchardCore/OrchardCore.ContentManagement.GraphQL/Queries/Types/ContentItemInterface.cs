using GraphQL.Types;
using Microsoft.Extensions.Localization;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public class ContentItemInterface : InterfaceGraphType<ContentItem>
    {
        public ContentItemInterface(IStringLocalizer<ContentItem> T)
        {
            Name = "ContentItem";

            Field(ci => ci.ContentItemId).Description(T["Content item id"]);
            Field(ci => ci.ContentItemVersionId).Description(T["The content item version id"]);
            Field(ci => ci.ContentType).Description(T["Type of content"]);
            Field(ci => ci.DisplayText).Description(T["The display text of the content item"]);
            Field(ci => ci.Published).Description(T["Is the published version"]);
            Field(ci => ci.Latest).Description(T["Is the latest version"]);
            Field<DateTimeGraphType>("modifiedUtc", resolve: ci => ci.Source.ModifiedUtc, description: T["The date and time of modification"]);
            Field<DateTimeGraphType>("publishedUtc", resolve: ci => ci.Source.PublishedUtc, description: T["The date and time of publication"]);
            Field<DateTimeGraphType>("createdUtc", resolve: ci => ci.Source.CreatedUtc, description: T["The date and time of creation"]);
            Field(ci => ci.Owner).Description(T["The owner of the content item"]);
            Field(ci => ci.Author).Description(T["The author of the content item"]);
        }
    }
}