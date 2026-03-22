using System.Collections.Concurrent;

namespace OrchardCore.ContentManagement.Display;

/// <summary>
/// Provides cached alternate patterns for ContentItem shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentItemAlternatesFactory
{
    private static readonly ConcurrentDictionary<DisplayAlternatesCacheKey, string[]> _displayCache = new();
    private static readonly ConcurrentDictionary<EditorAlternatesCacheKey, string[]> _editorCache = new();

    /// <summary>
    /// Gets or creates cached alternates for a ContentItem display shape.
    /// </summary>
    public static string[] GetDisplayAlternates(
        string stereotype,
        bool hasStereotype,
        string displayType,
        string actualShapeType,
        string contentType)
    {
        var key = new DisplayAlternatesCacheKey(
            stereotype ?? string.Empty,
            hasStereotype,
            displayType,
            actualShapeType,
            contentType);

        return _displayCache.GetOrAdd(key, BuildDisplayAlternates);
    }

    /// <summary>
    /// Gets or creates cached alternates for a ContentItem editor shape.
    /// </summary>
    public static string[] GetEditorAlternates(
        string stereotype,
        bool hasStereotype,
        string actualShapeType,
        string contentType)
    {
        var key = new EditorAlternatesCacheKey(
            stereotype ?? string.Empty,
            hasStereotype,
            actualShapeType,
            contentType);

        return _editorCache.GetOrAdd(key, BuildEditorAlternates);
    }

    private static string[] BuildDisplayAlternates(DisplayAlternatesCacheKey key)
    {
        var alternates = new List<string>();

        if (key.HasStereotype)
        {
            if (key.DisplayType != OrchardCoreConstants.DisplayType.Detail)
            {
                // Add fallback/default alternate Stereotype_[DisplayType] e.g. Content.Summary
                alternates.Add($"Stereotype_{key.DisplayType}");

                // [Stereotype]_[DisplayType] e.g. Menu.Summary
                alternates.Add($"{key.Stereotype}_{key.DisplayType}");
            }
            else
            {
                // Add fallback/default alternate i.e. Content 
                alternates.Add("Stereotype");

                // Add alternate to make the type [Stereotype] e.g. Menu
                alternates.Add(key.Stereotype);
            }
        }

        // Add alternate for [Stereotype]_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
        alternates.Add($"{key.ActualShapeType}__{key.ContentType}");

        return alternates.ToArray();
    }

    private static string[] BuildEditorAlternates(EditorAlternatesCacheKey key)
    {
        var alternates = new List<string>();

        if (key.HasStereotype)
        {
            // Add fallback/default alternate for Stereotype_Edit e.g. Stereotype.Edit
            alternates.Add("Stereotype_Edit");

            // add [Stereotype]_Edit e.g. Menu.Edit
            alternates.Add(key.ActualShapeType);
        }

        // Add an alternate for [Stereotype]_Edit__[ContentType] e.g. Content-Menu.Edit
        alternates.Add(key.ActualShapeType + "__" + key.ContentType);

        return alternates.ToArray();
    }

    internal readonly record struct DisplayAlternatesCacheKey(
        string Stereotype,
        bool HasStereotype,
        string DisplayType,
        string ActualShapeType,
        string ContentType);

    internal readonly record struct EditorAlternatesCacheKey(
        string Stereotype,
        bool HasStereotype,
        string ActualShapeType,
        string ContentType);
}
