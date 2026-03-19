using System.Collections.Concurrent;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Provides cached alternate patterns for content parts.
/// Alternates are computed once per unique part configuration and cached for reuse.
/// </summary>
public static class ContentPartAlternatesFactory
{
    private const string DisplayToken = "_Display";

    private static readonly ConcurrentDictionary<PartAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a content part configuration.
    /// </summary>
    /// <param name="definition">The content type part definition.</param>
    /// <param name="shapeType">The shape type being rendered.</param>
    /// <returns>A cached array of alternates.</returns>
    public static string[] GetAlternates(ContentTypePartDefinition definition, string shapeType, string displayType)
    {
        var key = new PartAlternatesCacheKey(
            definition.ContentTypeDefinition.Name,
            definition.PartDefinition.Name,
            definition.Name,
            definition.Editor() ?? string.Empty,
            definition.DisplayMode() ?? string.Empty,
            definition.ContentTypeDefinition.GetStereotype() ?? string.Empty,
            shapeType,
            displayType ?? string.Empty);

        return _cache.GetOrAdd(key, BuildAlternates);
    }

    /// <summary>
    /// Cache key for part alternates, using a readonly record struct for efficient hashing and equality.
    /// </summary>
    internal readonly record struct PartAlternatesCacheKey(
        string ContentType,
        string PartType,
        string PartName,
        string Editor,
        string DisplayMode,
        string Stereotype,
        string ShapeType,
        string DisplayType);

    private static string[] BuildAlternates(PartAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var editorPartType = !string.IsNullOrEmpty(key.Editor)
            ? key.PartType + "_Edit__" + key.Editor
            : key.PartType + "_Edit";
        var hasDisplayMode = !string.IsNullOrEmpty(key.DisplayMode);
        var isDisplayModeShapeType = key.ShapeType == key.PartType + "_Display__" + key.DisplayMode;
        var isPartTypeShape = key.ShapeType == key.PartType;
        var isEditorShape = key.ShapeType == editorPartType;
        var hasStereotype = !string.IsNullOrEmpty(key.Stereotype);
        var partTypeEqualsPartName = key.PartType == key.PartName;

        string[] displayTypes;

        if (isEditorShape)
        {
            displayTypes = ["_" + key.DisplayType];
        }
        else
        {
            displayTypes = ["", "_" + key.DisplayType];

            if (!isDisplayModeShapeType)
            {
                // [ShapeType]_[DisplayType], e.g. HtmlBodyPart.Summary, BagPart.Summary, ListPartFeed.Summary
                alternates.Add($"{key.ShapeType}_{key.DisplayType}");
            }
        }

        if (isPartTypeShape || isEditorShape)
        {
            foreach (var dt in displayTypes)
            {
                // [ContentType]_[DisplayType]__[PartType], e.g. Blog-HtmlBodyPart, LandingPage-BagPart
                alternates.Add($"{key.ContentType}{dt}__{key.PartType}");

                if (hasStereotype)
                {
                    // [Stereotype]__[DisplayType]__[PartType], e.g. Widget-ContentsMetadata
                    alternates.Add($"{key.Stereotype}{dt}__{key.PartType}");
                }
            }

            if (!partTypeEqualsPartName)
            {
                foreach (var dt in displayTypes)
                {
                    // [ContentType]_[DisplayType]__[PartName], e.g. LandingPage-Services
                    alternates.Add($"{key.ContentType}{dt}__{key.PartName}");

                    if (hasStereotype)
                    {
                        // [Stereotype]_[DisplayType]__[PartType]__[PartName], e.g. Widget-ServicePart-Services
                        alternates.Add($"{key.Stereotype}{dt}__{key.PartType}__{key.PartName}");
                    }
                }
            }
        }
        else
        {
            if (hasDisplayMode)
            {
                // [PartType]_[DisplayType]__[DisplayMode]_Display, e.g. HtmlBodyPart-MyDisplayMode.Display.Summary
                alternates.Add($"{key.PartType}_{key.DisplayType}__{key.DisplayMode}{DisplayToken}");
            }

            var lastAlternatesOfNamedPart = new List<string>();

            foreach (var dt in displayTypes)
            {
                var shapeTypeSuffix = key.ShapeType;
                var displayTypeDisplayToken = dt;

                if (hasDisplayMode)
                {
                    if (isDisplayModeShapeType)
                    {
                        shapeTypeSuffix = key.DisplayMode;
                    }
                    else
                    {
                        shapeTypeSuffix = $"{shapeTypeSuffix}__{key.DisplayMode}";
                    }

                    if (dt == "")
                    {
                        displayTypeDisplayToken = DisplayToken;
                    }
                    else
                    {
                        shapeTypeSuffix = $"{shapeTypeSuffix}{DisplayToken}";
                    }
                }

                // [ContentType]_[DisplayType]__[PartType]__[ShapeType], e.g. Blog-ListPart-ListPartFeed
                alternates.Add($"{key.ContentType}{displayTypeDisplayToken}__{key.PartType}__{shapeTypeSuffix}");

                if (hasStereotype)
                {
                    // [Stereotype]_[DisplayType]__[PartType]__[ShapeType], e.g. Blog-ListPart-ListPartFeed
                    alternates.Add($"{key.Stereotype}{displayTypeDisplayToken}__{key.PartType}__{shapeTypeSuffix}");
                }

                if (!partTypeEqualsPartName)
                {
                    // [ContentType]_[DisplayType]__[PartName]__[ShapeType], e.g. LandingPage-Services-BagPartSummary
                    lastAlternatesOfNamedPart.Add($"{key.ContentType}{displayTypeDisplayToken}__{key.PartName}__{shapeTypeSuffix}");

                    if (hasStereotype)
                    {
                        // [Stereotype]_[DisplayType]__[PartType]__[PartName]__[ShapeType], e.g. Widget-ServicePart-Services-BagPartSummary
                        lastAlternatesOfNamedPart.Add($"{key.Stereotype}{displayTypeDisplayToken}__{key.PartType}__{key.PartName}__{shapeTypeSuffix}");
                    }
                }
            }

            alternates.AddRange(lastAlternatesOfNamedPart);
        }

        return alternates.ToArray();
    }
}
