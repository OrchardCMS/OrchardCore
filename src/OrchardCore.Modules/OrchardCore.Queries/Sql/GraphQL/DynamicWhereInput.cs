using System;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;

namespace OrchardCore.Queries.Sql.GraphQL;

public class DynamicWhereInput : WhereInputObjectGraphType
{
    public DynamicWhereInput(TypeFields typeFields)
    {
        if (typeFields == null)
        {
            throw new ArgumentNullException(nameof(typeFields));
        }

        foreach (var field in typeFields)
        {
            if (field.GetMetadata<bool>("Filterable"))
            {
                AddScalarFilterFields(field.Type, field.Name, field.Description);
            }
        }
    }
}
