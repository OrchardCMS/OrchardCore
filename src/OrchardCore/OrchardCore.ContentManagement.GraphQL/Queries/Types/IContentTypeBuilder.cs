using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public interface IContentTypeBuilder
    {
        void Build(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType);
        void Clear() { }
    }
}
