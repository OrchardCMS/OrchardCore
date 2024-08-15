using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Types;

public sealed class DynamicPartWhereInputGraphType : WhereInputObjectGraphType<ContentPart>
{
    private ContentTypePartDefinition _part;

    public DynamicPartWhereInputGraphType(ContentTypePartDefinition part)
    {
        Name = $"{part.Name}WhereInput";
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
                        AddScalarFilterFields(fieldType.Type, fieldType.Name, fieldType.Description);
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
