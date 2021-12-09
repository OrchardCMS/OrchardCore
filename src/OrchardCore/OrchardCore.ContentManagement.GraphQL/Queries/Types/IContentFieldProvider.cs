using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types
{
    public interface IContentFieldProvider
    {
        FieldType GetField(ContentPartFieldDefinition field);
    }
}
