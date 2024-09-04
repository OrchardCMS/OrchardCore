using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartGraphType : ObjectGraphType<ContentPart>
{
    private ContentTypePartDefinition _part;

    public DynamicPartGraphType(ContentTypePartDefinition part)
    {
        Name = part.Name;
        _part = part;
    }

    public override void Initialize(ISchema schema)
    {
        if (schema is IServiceProvider serviceProvider)
        {
            var contentFieldProviders = serviceProvider.GetServices<IContentFieldProvider>().ToList();

            foreach (var field in _part.PartDefinition.Fields)
            {
                foreach (var fieldProvider in contentFieldProviders)
                {
                    var fieldType = fieldProvider.GetField(schema, field, _part.Name);
                    if (fieldType != null)
                    {
                        AddField(fieldType);
                        break;
                    }
                }
            }
        }

        // Part is not required here anymore, do not keep it alive.
        _part = null;

        base.Initialize(schema);
    }
}
