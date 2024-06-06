using GraphQL.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public interface IContentItemObjectGraphType : IObjectGraphType
{
    void AddOutputField(FieldType fieldType);
}

public interface IContentItemInputObjectGraphType : IInputObjectGraphType
{
    void AddInputField(FieldType fieldType);
}
