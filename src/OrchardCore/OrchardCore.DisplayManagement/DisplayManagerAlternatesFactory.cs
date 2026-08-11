using System.Collections.Concurrent;

namespace OrchardCore.DisplayManagement;

internal static class DisplayManagerAlternatesFactory
{
    private static readonly ConcurrentDictionary<DisplayAlternatesCacheKey, string[]> s_displayCache = new();
    private static readonly ConcurrentDictionary<EditorAlternatesCacheKey, string[]> s_editorCache = new();

    public static string[] GetDisplayAlternates(string actualShapeType, string modelTypeName)
    {
        var key = new DisplayAlternatesCacheKey(actualShapeType, modelTypeName);
        return s_displayCache.GetOrAdd(key, static k => [$"{k.ActualShapeType}__{k.ModelTypeName}"]);
    }

    public static string[] GetEditorAlternates(string actualShapeType, string modelTypeName)
    {
        var key = new EditorAlternatesCacheKey(actualShapeType, modelTypeName);

        return s_editorCache.GetOrAdd(key, static k =>
        [
            $"{k.ModelTypeName}_Edit",
            $"{k.ActualShapeType}__{k.ModelTypeName}"
        ]);
    }

    private readonly record struct DisplayAlternatesCacheKey(
        string ActualShapeType,
        string ModelTypeName);

    private readonly record struct EditorAlternatesCacheKey(
        string ActualShapeType,
        string ModelTypeName);
}
