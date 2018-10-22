using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Mutations
{
    public interface IContentTypeMutationBuilder
    {
        void BuildAsync(FieldType mutation, ContentTypeDefinition contentTypeDefinition, InputObjectGraphType<ContentItem> contentItemType);
    }
}