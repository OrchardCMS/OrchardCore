using System.Collections.Generic;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartInputGraphType : WhereInputObjectGraphType<ContentPart>
{
    public DynamicPartInputGraphType(
        ContentTypePartDefinition part,
        IEnumerable<FieldType> scalers)
    {
        Name = $"{part.Name}WhereInput"; ;

        foreach (var scaler in scalers)
        {
            AddScalarFilterFields(scaler.Type, scaler.Name, scaler.Description);
        }
    }
}
