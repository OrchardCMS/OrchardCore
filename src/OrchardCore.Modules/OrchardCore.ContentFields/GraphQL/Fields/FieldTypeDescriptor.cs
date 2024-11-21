using GraphQL.Types;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.GraphQL.Fields;

internal sealed class FieldTypeDescriptor
{
    public string Description { get; set; }

    public Type FieldType { get; set; }

    public Type UnderlyingType { get; set; }

    public required IGraphType ResolvedType { get; set; }

    public Func<ContentElement, object> FieldAccessor { get; set; }

    public string Index { get; set; }

    public Type IndexType { get; set; }
}
