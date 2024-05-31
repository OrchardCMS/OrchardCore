using System;
using System.Linq;
using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL;

public static class GraphQLTypeExtensions
{
    public static FieldType WithPartCollapsedMetaData(this FieldType fieldType, bool collapsed = true)
        => fieldType.WithMetaData("PartCollapsed", collapsed);

    public static FieldType WithPartNameMetaData(this FieldType fieldType, string partName)
        => fieldType.WithMetaData("PartName", partName);

    public static bool HasFieldIgnoreCase(this IComplexGraphType graphType, string fieldName)
        => graphType.Fields.Any(field => field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    private static FieldType WithMetaData(this FieldType fieldType, string name, object value)
    {
        // TODO: Understand if locking is the best solution to https://github.com/OrchardCMS/OrchardCore/issues/15308
        lock (fieldType.Metadata)
        {
            fieldType.Metadata.TryAdd(name, value);
        }

        return fieldType;
    }
}
