using GraphQL.Types;

namespace OrchardCore.Apis.GraphQL.Queries;

public interface IFilterInputObjectGraphType : IInputObjectGraphType
{
    void AddScalarFilterFields<TGraphType>(string fieldName, string description);

    void AddScalarFilterFields<TGraphType>(string fieldName, string description, string aliasName)
        => AddScalarFilterFields<TGraphType>(fieldName, description);

    void AddScalarFilterFields(Type graphType, string fieldName, string description);

    void AddScalarFilterFields(Type graphType, string fieldName, string description, string aliasName)
        => AddScalarFilterFields(graphType, fieldName, description);
}
