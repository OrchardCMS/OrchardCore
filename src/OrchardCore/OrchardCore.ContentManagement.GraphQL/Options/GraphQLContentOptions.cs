using GraphQL;
using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Settings;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.GraphQL.Options;

public class GraphQLContentOptions
{
    public IEnumerable<GraphQLContentTypeOption> ContentTypeOptions { get; set; } = [];

    public IEnumerable<GraphQLContentPartOption> PartOptions { get; set; } = [];

    public IEnumerable<GraphQLField> HiddenFields { get; set; } = [];

    /// <summary>
    /// By default, only content types without stereotypes are included.
    /// This option enables you to explicitly add additional stereotypes for discovery.
    /// </summary>
    public HashSet<string> DiscoverableSterotypes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public GraphQLContentOptions ConfigureContentType(string contentType, Action<GraphQLContentTypeOption> action)
    {
        var option = new GraphQLContentTypeOption(contentType);

        action(option);

        ContentTypeOptions = ContentTypeOptions.Union([option]);

        return this;
    }

    public GraphQLContentOptions ConfigurePart<TContentPart>(Action<GraphQLContentPartOption> action)
        where TContentPart : ContentPart
    {
        var option = new GraphQLContentPartOption<TContentPart>();

        action(option);

        PartOptions = PartOptions.Union([option]);

        return this;
    }

    public GraphQLContentOptions ConfigurePart(string partName, Action<GraphQLContentPartOption> action)
    {
        var option = new GraphQLContentPartOption(partName);

        action(option);

        PartOptions = PartOptions.Union([option]);

        return this;
    }

    public GraphQLContentOptions IgnoreField<TGraphType>(string fieldName) where TGraphType : IObjectGraphType
    {
        HiddenFields = HiddenFields.Union([new GraphQLField<TGraphType>(fieldName)]);

        return this;
    }

    public GraphQLContentOptions IgnoreField(Type fieldType, string fieldName)
    {
        HiddenFields = HiddenFields.Union([new GraphQLField(fieldType, fieldName)]);

        return this;
    }

    /// <summary>
    /// Collapsing works at a hierarchy
    ///
    /// If the Content Type is marked at collapsed, then all parts are collapsed.
    /// If the Content Type is not marked collapsed, then it falls down to the content type under it.
    /// If the Content Part at a top level is marked collapsed, then it will trump above.
    /// </summary>
    /// <param name="definition"></param>
    /// <returns></returns>
    internal bool ShouldCollapse(ContentTypePartDefinition definition)
    {
        if (IsCollapsedByDefault(definition))
        {
            return true;
        }

        var settings = definition.GetSettings<GraphQLContentTypePartSettings>();

        if (settings.Collapse)
        {
            return true;
        }

        return false;
    }

    public bool IsCollapsedByDefault(ContentTypePartDefinition definition)
    {
        var contentType = definition.ContentTypeDefinition.Name;
        var partName = definition.PartDefinition.Name;

        if (contentType == partName)
        {
            return true;
        }

        var contentTypeOption = ContentTypeOptions.FirstOrDefault(ctp => ctp.ContentType == contentType);

        if (contentTypeOption != null)
        {
            if (contentTypeOption.Collapse)
            {
                return true;
            }

            var contentTypePartOption = contentTypeOption.PartOptions.FirstOrDefault(p => p.Name == partName);

            if (contentTypePartOption != null)
            {
                if (contentTypePartOption.Collapse)
                {
                    return true;
                }
            }
        }

        var contentPartOption = PartOptions.FirstOrDefault(p => p.Name == partName);

        if (contentPartOption != null)
        {
            if (contentPartOption.Collapse)
            {
                return true;
            }
        }

        return false;
    }

    internal bool ShouldSkip(ContentTypePartDefinition definition)
    {
        if (IsHiddenByDefault(definition))
        {
            return true;
        }

        var settings = definition.GetSettings<GraphQLContentTypePartSettings>();

        if (settings.Hidden)
        {
            return true;
        }

        return false;
    }

    public bool IsHiddenByDefault(string contentType)
    {
        ArgumentException.ThrowIfNullOrEmpty(contentType);

        var contentTypeOption = ContentTypeOptions.FirstOrDefault(ctp => ctp.ContentType == contentType);

        return contentTypeOption?.Hidden ?? false;
    }

    public bool ShouldHide(ContentTypeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var settings = definition.GetSettings<GraphQLContentTypeSettings>();

        if (settings.Hidden)
        {
            return true;
        }

        return IsHiddenByDefault(definition.Name);
    }

    internal bool ShouldSkip(Type fieldType, string fieldName)
    {
        return HiddenFields
            .Any(x => x.FieldType == fieldType && x.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsHiddenByDefault(ContentTypePartDefinition definition)
    {
        var contentType = definition.ContentTypeDefinition.Name;
        var partName = definition.PartDefinition.Name;

        var contentTypeOption = ContentTypeOptions.FirstOrDefault(ctp => ctp.ContentType == contentType);

        if (contentTypeOption != null)
        {
            if (contentTypeOption.Hidden)
            {
                return true;
            }

            var contentTypePartOption = contentTypeOption.PartOptions.FirstOrDefault(p => p.Name == partName);

            if (contentTypePartOption != null)
            {
                if (contentTypePartOption.Hidden)
                {
                    return true;
                }
            }
        }

        var contentPartOption = PartOptions.FirstOrDefault(p => p.Name == partName);

        if (contentPartOption != null)
        {
            if (contentPartOption.Hidden)
            {
                return true;
            }
        }

        return false;
    }

    internal static string GetFieldName(ContentTypePartDefinition definition, string partName, string fieldName)
    {
        var settings = definition.GetSettings<GraphQLContentTypePartSettings>();

        if (settings.PreventFieldNameCollision)
        {
            return partName.ToFieldName() + fieldName.ToPascalCase();
        }

        return fieldName.ToCamelCase();
    }
}
