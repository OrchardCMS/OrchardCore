using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Spatial.Fields;

namespace OrchardCore.Spatial.GraphQL;
public class GeoPointFieldProvider : IContentFieldProvider
{
    protected readonly IStringLocalizer S;
    public GeoPointFieldProvider(IStringLocalizer<GeoPointFieldProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    private static object HandleFieldResolving(ContentElement field)
        => new { Latitude = ((GeoPointField)field).Latitude, Longitude = ((GeoPointField)field).Longitude };

    public FieldType GetField(ISchema schema, ContentPartFieldDefinition field, string namedPartTechnicalName, string customFieldName = null)
    {

        return new FieldType
        {
            Name = customFieldName ?? schema.NameConverter.NameForField(field.Name, null),
            Description = S["Geo point field"],
            Type = typeof(GeoPointGraphType),
            ResolvedType = new GeoPointGraphType(),
            Resolver = new FuncFieldResolver<ContentElement, object>(context =>
            {
                // Check if part has been collapsed by trying to get the parent part.
                ContentElement contentPart = context.Source.Get<ContentPart>(field.PartDefinition.Name);

                // Part is not collapsed, access field directly.
                contentPart ??= context.Source;

                var contentField = contentPart?.Get(typeof(GeoPointField), field.Name);

                contentField ??= context.Source.Get(typeof(GeoPointField), field.Name);

                return contentField == null ? null : HandleFieldResolving(contentField);
            }),
        };
    }

    public FieldTypeIndexDescriptor GetFieldIndex(ContentPartFieldDefinition field)
    {
        return null;
    }

    public bool HasField(ISchema schema, ContentPartFieldDefinition field)
    {
        if (field.FieldDefinition.Name == nameof(GeoPointField)) return true;
        return false;
    }

    public bool HasFieldIndex(ContentPartFieldDefinition field)
        => false;
}
