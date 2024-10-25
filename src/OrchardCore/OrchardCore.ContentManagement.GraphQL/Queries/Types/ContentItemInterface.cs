using GraphQL.Types;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.GraphQL.Options;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public class ContentItemInterface : InterfaceGraphType<ContentItem>
{
    private readonly GraphQLContentOptions _options;

    public ContentItemInterface(IOptions<GraphQLContentOptions> optionsAccessor)
    {
        _options = optionsAccessor.Value;

        Name = "ContentItem";

        Field(ci => ci.ContentItemId);
        Field(ci => ci.ContentItemVersionId, nullable: true);
        Field(ci => ci.ContentType);
        Field(ci => ci.DisplayText, nullable: true);
        Field(ci => ci.Published);
        Field(ci => ci.Latest);
        Field(ci => ci.ModifiedUtc, nullable: true);
        Field(ci => ci.PublishedUtc, nullable: true);
        Field(ci => ci.CreatedUtc, nullable: true);
        Field(ci => ci.Owner, nullable: true);
        Field(ci => ci.Author, nullable: true);
    }

    public override FieldType AddField(FieldType fieldType)
    {
        if (!_options.ShouldSkip(typeof(ContentItemType), fieldType.Name))
        {
            return base.AddField(fieldType);
        }

        return null;
    }
}
