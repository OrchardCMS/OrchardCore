using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL;

public static class GraphQLTypeExtensions
{
    public static FieldType WithPartCollapsedMetaData(this FieldType fieldType, bool collapsed = true)
        => fieldType.WithMetaData("PartCollapsed", collapsed);

    public static FieldType WithPartNameMetaData(this FieldType fieldType, string partName)
        => fieldType.WithMetaData("PartName", partName);

    public static FieldType WithAliasNameMetaData(this FieldType fieldType, string aliasName)
        => fieldType.WithMetaData("AliasName", aliasName);

    public static FieldType WithContentPartMetaData(this FieldType fieldType, string contentPart)
        => fieldType.WithMetaData("ContentPart", contentPart);

    public static FieldType WithContentFieldMetaData(this FieldType fieldType, string contentField)
        => fieldType.WithMetaData("ContentField", contentField);

    /// <summary>
    /// Checks if the field exists in the GraphQL type in a case-insensitive way.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the same as calling <see cref="IComplexGraphType.HasField(string)"/> but in a case-insensitive way. OC
    /// fields may be added with different casings, and we want to avoid collisions even then.
    /// </para>
    /// <para>
    /// See <see href="https://github.com/OrchardCMS/OrchardCore/pull/16151"/> and its corresponding issues for context.
    /// </para>
    /// </remarks>
    public static bool HasFieldIgnoreCase(this IComplexGraphType graphType, string fieldName)
        => graphType.Fields.Any(field => field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

    private static FieldType WithMetaData(this FieldType fieldType, string name, object value)
    {
        if (value != null)
        {
            // TODO: Understand if locking is the best solution to https://github.com/OrchardCMS/OrchardCore/issues/15308
            lock (fieldType.Metadata)
            {
                fieldType.Metadata.TryAdd(name, value);
            }
        }

        return fieldType;
    }
}
