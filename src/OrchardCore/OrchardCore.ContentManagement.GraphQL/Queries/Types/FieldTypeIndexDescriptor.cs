namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class FieldTypeIndexDescriptor
{
    public required string Index { get; set; }

    public required Type IndexType { get; set; }
}
