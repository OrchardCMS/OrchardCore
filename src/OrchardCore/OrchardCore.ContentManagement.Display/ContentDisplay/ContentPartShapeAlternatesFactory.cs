using System.Collections.Concurrent;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Provides cached alternate patterns for ContentPart shapes in ContentItemDisplayCoordinator.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentPartShapeAlternatesFactory
{
    private static readonly ConcurrentDictionary<DisplayAlternatesCacheKey, string[]> _displayCache = new();
    private static readonly ConcurrentDictionary<EditorAlternatesCacheKey, string[]> _editorCache = new();

    /// <summary>
    /// Gets or creates cached alternates for a ContentPart display shape.
    /// </summary>
    public static string[] GetDisplayAlternates(
        string contentType,
        string partTypeName,
        string partName,
        string stereotype,
        bool hasStereotype,
        string displayType)
    {
        var key = new DisplayAlternatesCacheKey(
            contentType,
            partTypeName,
            partName,
            stereotype ?? string.Empty,
            hasStereotype,
            displayType);

        return _displayCache.GetOrAdd(key, BuildDisplayAlternates);
    }

    /// <summary>
    /// Gets or creates cached alternates for a ContentPart_Edit shape.
    /// </summary>
    public static string[] GetEditorAlternates(
        string contentType,
        string partTypeName,
        string partName,
        bool isNamedPart)
    {
        var key = new EditorAlternatesCacheKey(
            contentType,
            partTypeName,
            partName,
            isNamedPart);

        return _editorCache.GetOrAdd(key, BuildEditorAlternates);
    }

    private static string[] BuildDisplayAlternates(DisplayAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var displayTypes = new[] { string.Empty, "_" + key.DisplayType };

        foreach (var displayType in displayTypes)
        {
            // eg. ServicePart,  ServicePart.Summary
            alternates.Add($"{key.PartTypeName}{displayType}");

            // [ContentType]_[DisplayType]__[PartType]
            // e.g. LandingPage-ServicePart, LandingPage-ServicePart.Summary
            alternates.Add($"{key.ContentType}{displayType}__{key.PartTypeName}");

            if (key.HasStereotype)
            {
                // [Stereotype]_[DisplayType]__[PartType],
                // e.g. Widget-ServicePart
                alternates.Add($"{key.Stereotype}{displayType}__{key.PartTypeName}");
            }
        }

        if (key.PartTypeName != key.PartName)
        {
            foreach (var displayType in displayTypes)
            {
                // [ContentType]_[DisplayType]__[PartName]
                // e.g. Employee-Address1, Employee-Address2
                alternates.Add($"{key.ContentType}{displayType}__{key.PartName}");

                if (key.HasStereotype)
                {
                    // [Stereotype]_[DisplayType]__[PartType]__[PartName]
                    // e.g. Widget-Services
                    alternates.Add($"{key.Stereotype}{displayType}__{key.PartTypeName}__{key.PartName}");
                }
            }
        }

        return alternates.ToArray();
    }

    private static string[] BuildEditorAlternates(EditorAlternatesCacheKey key)
    {
        const string shapeType = "ContentPart_Edit";

        var alternates = new List<string>
        {
            // ContentPart_Edit__[PartType]
            // eg ContentPart-ServicePart.Edit
            $"{shapeType}__{key.PartTypeName}",

            // ContentPart_Edit__[ContentType]__[PartType]
            // e.g. ContentPart-LandingPage-ServicePart.Edit
            $"{shapeType}__{key.ContentType}__{key.PartTypeName}",
        };

        if (key.IsNamedPart)
        {
            // ContentPart_Edit__[ContentType]__[PartName]
            // e.g. ContentPart-LandingPage-BillingService.Edit ContentPart-LandingPage-HelplineService.Edit
            alternates.Add($"{shapeType}__{key.ContentType}__{key.PartName}");
        }

        return alternates.ToArray();
    }

    internal readonly record struct DisplayAlternatesCacheKey(
        string ContentType,
        string PartTypeName,
        string PartName,
        string Stereotype,
        bool HasStereotype,
        string DisplayType);

    internal readonly record struct EditorAlternatesCacheKey(
        string ContentType,
        string PartTypeName,
        string PartName,
        bool IsNamedPart);
}
