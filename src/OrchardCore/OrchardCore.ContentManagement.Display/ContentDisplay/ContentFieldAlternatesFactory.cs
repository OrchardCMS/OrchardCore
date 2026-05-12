using System.Collections.Concurrent;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Provides cached alternate patterns for content fields.
/// Alternates are computed once per unique field configuration and cached for reuse.
/// </summary>
public static class ContentFieldAlternatesFactory
{
    private const string DisplayToken = "_Display";

    private static readonly ConcurrentDictionary<FieldAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a content field configuration.
    /// </summary>
    /// <param name="typePartDefinition">The content type part definition.</param>
    /// <param name="partFieldDefinition">The content part field definition.</param>
    /// <param name="shapeType">The shape type being rendered.</param>
    /// <returns>A cached array of alternates.</returns>
    public static string[] GetAlternates(
        ContentTypePartDefinition typePartDefinition,
        ContentPartFieldDefinition partFieldDefinition,
        string shapeType,
        string displayType)
    {
        var key = new FieldAlternatesCacheKey(
            typePartDefinition.ContentTypeDefinition.Name,
            typePartDefinition.PartDefinition.Name,
            typePartDefinition.Name,
            partFieldDefinition.FieldDefinition.Name,
            partFieldDefinition.Name,
            partFieldDefinition.Editor() ?? string.Empty,
            partFieldDefinition.DisplayMode() ?? string.Empty,
            shapeType,
            displayType ?? string.Empty);

        return _cache.GetOrAdd(key, BuildAlternates);
    }

    /// <summary>
    /// Cache key for field alternates, using a readonly record struct for efficient hashing and equality.
    /// </summary>
    internal readonly record struct FieldAlternatesCacheKey(
        string ContentType,
        string PartType,
        string PartName,
        string FieldType,
        string FieldName,
        string Editor,
        string DisplayMode,
        string ShapeType,
        string DisplayType);

    private static string[] BuildAlternates(FieldAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var displayTypes = new[] { string.Empty, "_" + key.DisplayType };
        var hasDisplayMode = !string.IsNullOrEmpty(key.DisplayMode);
        var isFieldTypeShape = key.ShapeType == key.FieldType;

        // [ShapeType]_[DisplayType], e.g. TextField.Summary
        alternates.Add($"{key.ShapeType}_{key.DisplayType}");

        if (isFieldTypeShape)
        {
            foreach (var dt in displayTypes)
            {
                // [PartType]__[FieldName], e.g. HtmlBodyPart-Description
                alternates.Add($"{key.PartType}{dt}__{key.FieldName}");

                // [ContentType]__[FieldType], e.g. Blog-TextField, LandingPage-TextField
                alternates.Add($"{key.ContentType}{dt}__{key.FieldType}");

                // [ContentType]__[PartName]__[FieldName], e.g. Blog-HtmlBodyPart-Description, LandingPage-Services-Description
                alternates.Add($"{key.ContentType}{dt}__{key.PartType}__{key.FieldName}");
            }
        }
        else
        {
            if (hasDisplayMode)
            {
                // [FieldType]_[DisplayType]__[DisplayMode]_Display, e.g. TextField-Header.Display.Summary
                alternates.Add($"{key.FieldType}_{key.DisplayType}__{key.DisplayMode}{DisplayToken}");
            }

            for (var i = 0; i < displayTypes.Length; i++)
            {
                var dt = displayTypes[i];
                var shapeType = key.ShapeType;

                if (hasDisplayMode)
                {
                    shapeType = $"{key.FieldType}__{key.DisplayMode}";

                    if (dt == string.Empty)
                    {
                        dt = DisplayToken;
                    }
                    else
                    {
                        shapeType += DisplayToken;
                    }
                }

                // [FieldType]__[ShapeType], e.g. TextField-TextFieldSummary
                alternates.Add($"{key.FieldType}{dt}__{shapeType}");

                // [PartType]__[FieldName]__[ShapeType], e.g. HtmlBodyPart-Description-TextFieldSummary
                alternates.Add($"{key.PartType}{dt}__{key.FieldName}__{shapeType}");

                // [ContentType]__[FieldType]__[ShapeType], e.g. Blog-TextField-TextFieldSummary, LandingPage-TextField-TextFieldSummary
                alternates.Add($"{key.ContentType}{dt}__{key.FieldType}__{shapeType}");

                // [ContentType]__[PartName]__[FieldName]__[ShapeType], e.g. Blog-HtmlBodyPart-Description-TextFieldSummary, LandingPage-Services-Description-TextFieldSummary
                alternates.Add($"{key.ContentType}{dt}__{key.PartName}__{key.FieldName}__{shapeType}");
            }
        }

        return alternates.ToArray();
    }
}
