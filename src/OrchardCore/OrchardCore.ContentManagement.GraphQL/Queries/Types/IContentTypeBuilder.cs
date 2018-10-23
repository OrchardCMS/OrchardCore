using System.Threading.Tasks;
using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public interface IContentTypeBuilder
    {
        void BuildAsync(FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType);
    }
}