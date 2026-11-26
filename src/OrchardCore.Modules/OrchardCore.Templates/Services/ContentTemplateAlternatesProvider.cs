using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Services;

/// <summary>
/// Suggests the alternates of the content items, parts and fields of the content types, in the display types the front
/// end uses, or the admin. The part and field alternates are those the content display drivers add.
/// </summary>
public sealed class ContentTemplateAlternatesProvider : ITemplateAlternatesProvider
{
    private const string DisplayModeSeparator = "_Display__";

    private static readonly string[] s_frontEndDisplayTypes = [OrchardCoreConstants.DisplayType.Detail, OrchardCoreConstants.DisplayType.Summary];
    private static readonly string[] s_adminDisplayTypes = [OrchardCoreConstants.DisplayType.DetailAdmin, OrchardCoreConstants.DisplayType.SummaryAdmin];

    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ContentTemplateAlternatesProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<TemplateAlternate>> GetAlternatesAsync(bool adminTemplates)
    {
        List<TemplateAlternate> alternates = [];
        var displayTypes = adminTemplates ? s_adminDisplayTypes : s_frontEndDisplayTypes;

        foreach (var typeDefinition in await _contentDefinitionManager.ListTypeDefinitionsAsync())
        {
            foreach (var displayType in displayTypes)
            {
                AddContentItemAlternates(alternates, typeDefinition, displayType);

                foreach (var typePartDefinition in typeDefinition.Parts)
                {
                    AddPartAlternates(alternates, typePartDefinition, displayType);

                    foreach (var partFieldDefinition in typePartDefinition.PartDefinition.Fields)
                    {
                        AddFieldAlternates(alternates, typePartDefinition, partFieldDefinition, displayType);
                    }
                }
            }
        }

        return alternates;
    }

    // The alternate ContentItemDisplayManager adds, [Stereotype]_[DisplayType]__[ContentType], e.g. Content_Summary__BlogPost.
    private static void AddContentItemAlternates(List<TemplateAlternate> alternates, ContentTypeDefinition typeDefinition, string displayType)
    {
        var shapeType = typeDefinition.TryGetStereotype(out var stereotype) ? stereotype : "Content";
        var displayShapeType = displayType == OrchardCoreConstants.DisplayType.Detail ? shapeType : $"{shapeType}_{displayType}";

        alternates.Add(new TemplateAlternate
        {
            Name = $"{displayShapeType}__{typeDefinition.Name}",
            ShapeType = shapeType,
            DisplayType = displayType,
            ContentType = typeDefinition.Name,
        });
    }

    private static void AddPartAlternates(List<TemplateAlternate> alternates, ContentTypePartDefinition typePartDefinition, string displayType)
    {
        // The shape a part driver renders by default, e.g. HtmlBodyPart, or HtmlBodyPart_Display__[DisplayMode].
        var displayMode = typePartDefinition.DisplayMode();
        var shapeType = string.IsNullOrEmpty(displayMode)
            ? typePartDefinition.PartDefinition.Name
            : typePartDefinition.PartDefinition.Name + DisplayModeSeparator + displayMode;

        foreach (var name in ContentPartAlternatesFactory.GetAlternates(typePartDefinition, shapeType, displayType))
        {
            alternates.Add(new TemplateAlternate
            {
                Name = name,
                ShapeType = shapeType,
                DisplayType = displayType,
                ContentType = typePartDefinition.ContentTypeDefinition.Name,
                Element = typePartDefinition.Name,
            });
        }
    }

    private static void AddFieldAlternates(
        List<TemplateAlternate> alternates,
        ContentTypePartDefinition typePartDefinition,
        ContentPartFieldDefinition partFieldDefinition,
        string displayType)
    {
        // The shape a field driver renders by default, e.g. TextField, or TextField_Display__[DisplayMode].
        var displayMode = partFieldDefinition.DisplayMode();
        var shapeType = string.IsNullOrEmpty(displayMode)
            ? partFieldDefinition.FieldDefinition.Name
            : partFieldDefinition.FieldDefinition.Name + DisplayModeSeparator + displayMode;

        foreach (var name in ContentFieldAlternatesFactory.GetAlternates(typePartDefinition, partFieldDefinition, shapeType, displayType))
        {
            alternates.Add(new TemplateAlternate
            {
                Name = name,
                ShapeType = shapeType,
                DisplayType = displayType,
                ContentType = typePartDefinition.ContentTypeDefinition.Name,
                Element = $"{typePartDefinition.Name}.{partFieldDefinition.Name}",
            });
        }
    }
}
