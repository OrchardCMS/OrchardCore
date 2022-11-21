using System;
using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.Queries.Sql.GraphQL;

public class DynamicOrderByInput : InputObjectGraphType
{
    public DynamicOrderByInput(TypeFields typeFields)
    {
        if (typeFields == null)
        {
            throw new ArgumentNullException(nameof(typeFields));
        }

        foreach (var field in typeFields)
        {
            if (field.GetMetadata<bool>("Sortable"))
            {
                Field<OrderByDirectionGraphType>(field.Name);
            }
        }
    }
}
