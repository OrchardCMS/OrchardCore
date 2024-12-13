using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Options;

public class GraphQLField<TGraphType> : GraphQLField where TGraphType : IObjectGraphType
{
    public GraphQLField(string fieldName) : base(typeof(TGraphType), fieldName)
    {
    }
}

public class GraphQLField
{
    public GraphQLField(Type fieldType, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(fieldType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

        FieldName = fieldName;
        FieldType = fieldType;
    }

    public string FieldName { get; }

    public Type FieldType { get; }
}
