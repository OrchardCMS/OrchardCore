using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL;

public static class FieldTypeExtensions
{
    public static FieldType WithPartCollapsedMetaData(this FieldType fieldType, bool collapsed = true)
    {
        return fieldType.WithMetaData("PartCollapsed", collapsed);
    }

    public static FieldType WithPartNameMetaData(this FieldType fieldType, string partName)
    {
        return fieldType.WithMetaData("PartName", partName);
    }

    private static FieldType WithMetaData(this FieldType fieldType, string name, object value)
    {
        lock (fieldType.Metadata)
        {
            fieldType.Metadata.TryAdd(name, value);
        }

        return fieldType;
    }
}
