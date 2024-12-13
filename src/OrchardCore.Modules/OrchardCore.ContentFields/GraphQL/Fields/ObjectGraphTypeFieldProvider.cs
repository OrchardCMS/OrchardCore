using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.GraphQL.Fields;

public class ObjectGraphTypeFieldProvider : IContentFieldProvider
{
    public ObjectGraphTypeFieldProvider()
    {
    }

    public FieldType GetField(ISchema schema, ContentPartFieldDefinition field, string namedPartTechnicalName, string customFieldName)
    {
        var queryGraphType = GetObjectGraphType(schema, field);

        if (queryGraphType != null)
        {
            return new FieldType
            {
                Name = customFieldName ?? field.Name,
                Description = field.FieldDefinition.Name,
                ResolvedType = queryGraphType,
                Resolver = new FuncFieldResolver<ContentElement, ContentElement>(context =>
                {
                    var typeToResolve = context.FieldDefinition.ResolvedType.GetType().BaseType
                        .GetGenericArguments().First();

                    // Check if part has been collapsed by trying to get the parent part.
                    ContentElement contentPart = context.Source.Get<ContentPart>(field.PartDefinition.Name);

                    // Part is not collapsed, access field directly.
                    contentPart ??= context.Source;

                    var contentField = contentPart?.Get(typeToResolve, field.Name);
                    return contentField;
                })
            };
        }

        return null;
    }

    public FieldTypeIndexDescriptor GetFieldIndex(ContentPartFieldDefinition field)
    {
        return null;
    }

    public bool HasField(ISchema schema, ContentPartFieldDefinition field) => GetObjectGraphType(schema, field) != null;

    public bool HasFieldIndex(ContentPartFieldDefinition field) => false;

    private static IObjectGraphType GetObjectGraphType(ISchema schema, ContentPartFieldDefinition field) =>
        schema.AdditionalTypeInstances
            .FirstOrDefault(x => x is IObjectGraphType && x.GetType().BaseType.GetGenericArguments().First().Name == field.FieldDefinition.Name) as IObjectGraphType;
}
