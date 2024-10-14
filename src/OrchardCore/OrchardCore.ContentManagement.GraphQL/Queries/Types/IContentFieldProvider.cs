using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public interface IContentFieldProvider
{
    FieldType GetField(ISchema schema, ContentPartFieldDefinition field, string namedPartTechnicalName, string customFieldName = null);

    bool HasField(ISchema schema, ContentPartFieldDefinition field);

    FieldTypeIndexDescriptor GetFieldIndex(ContentPartFieldDefinition field);

    bool HasFieldIndex(ContentPartFieldDefinition field);
}

public sealed class FieldTypeIndexDescriptor
{
    public required string Index { get; set; }
    public required Type IndexType { get; set; }
}
