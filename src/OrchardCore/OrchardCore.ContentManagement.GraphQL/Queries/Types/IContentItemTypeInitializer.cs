using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public interface IContentItemTypeInitializer
{
    void Initialize(ContentItemType contentItemType, ISchema schema);
}
