using GraphQL.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartGraphType : ObjectGraphType<ContentPart>
{
    private ContentTypePartDefinition _part;
    private readonly IEnumerable<IContentFieldProvider> _contentFieldProviders;

    public DynamicPartGraphType(
        ContentTypePartDefinition part,
        IEnumerable<IContentFieldProvider> contentFieldProviders)
    {
        Name = part.Name;
        _part = part;
        _contentFieldProviders = contentFieldProviders;
    }

    public override void Initialize(ISchema schema)
    {
        foreach (var field in _part.PartDefinition.Fields)
        {
            foreach (var fieldProvider in _contentFieldProviders)
            {
                var fieldType = fieldProvider.GetField(schema, field, _part.Name);
                if (fieldType != null)
                {
                    AddField(fieldType);
                    break;
                }
            }
        }

        // Part is not required here anymore, do not keep it alive.
        _part = null;

        base.Initialize(schema);
    }
}
