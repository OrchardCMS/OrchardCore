using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartWhereInputGraphType : WhereInputObjectGraphType<ContentPart>
{
    public DynamicPartWhereInputGraphType(
        ContentTypePartDefinition part,
        ISchema schema,
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        IStringLocalizer<DynamicPartWhereInputGraphType> stringLocalizer)
        : base(stringLocalizer)
    {
        Name = $"{part.Name}WhereInput";

        foreach (var field in part.PartDefinition.Fields)
        {
            foreach (var fieldProvider in contentFieldProviders)
            {
                var fieldType = fieldProvider.GetField(schema, field, part.Name);

                if (fieldType != null)
                {
                    AddScalarFilterFields(fieldType.Type, fieldType.Name, fieldType.Description);
                    break;
                }
            }
        }
    }
}
