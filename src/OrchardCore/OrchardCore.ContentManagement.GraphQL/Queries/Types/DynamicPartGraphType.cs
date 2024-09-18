using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartGraphType : ObjectGraphType<ContentPart>
{
    public DynamicPartGraphType(
        ContentTypePartDefinition part,
        IEnumerable<IContentFieldProvider> contentFieldProviders,
        ISchema schema)
    {
        Name = part.Name;

        foreach (var field in part.PartDefinition.Fields)
        {
            foreach (var fieldProvider in contentFieldProviders)
            {
                var fieldType = fieldProvider.GetField(schema, field, part.Name);
                if (fieldType == null)
                {
                    break;
                }

                AddField(fieldType);
            }
        }
    }
}
