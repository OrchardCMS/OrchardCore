using System.Collections.Concurrent;

namespace OrchardCore.Widgets;

internal static class ContentCardAlternatesFactory
{
    private static readonly ConcurrentDictionary<ContentCardAlternatesCacheKey, string[]> s_editCache = new();
    private static readonly ConcurrentDictionary<ContentCardAlternatesCacheKey, string[]> s_frameCache = new();
    private static readonly ConcurrentDictionary<string, string[]> s_fieldsEditCache = new(StringComparer.Ordinal);

    private const string ContentCardEdit = "ContentCard_Edit";
    private const string ContentCardFrame = "ContentCard_Frame";

    public static string[] GetEditAlternates(string collectionType, string contentType, string parentContentType, string namedPart)
    {
        var key = new ContentCardAlternatesCacheKey(
            collectionType ?? string.Empty,
            contentType ?? string.Empty,
            parentContentType ?? string.Empty,
            namedPart ?? string.Empty);

        return s_editCache.GetOrAdd(key, static k => BuildAlternates(ContentCardEdit, k));
    }

    public static string[] GetFrameAlternates(string collectionType, string contentType, string parentContentType, string namedPart)
    {
        var key = new ContentCardAlternatesCacheKey(
            collectionType ?? string.Empty,
            contentType ?? string.Empty,
            parentContentType ?? string.Empty,
            namedPart ?? string.Empty);

        return s_frameCache.GetOrAdd(key, static k => BuildAlternates(ContentCardFrame, k));
    }

    public static string[] GetFieldsEditAlternates(string collectionType)
    {
        return s_fieldsEditCache.GetOrAdd(collectionType, static c => [$"{c}_Fields_Edit"]);
    }

    private static string[] BuildAlternates(string shapeType, ContentCardAlternatesCacheKey key)
    {
        var alternates = new List<string>
        {
            $"{shapeType}__{key.CollectionType}",
            $"{shapeType}__{key.ContentType}",
            $"{shapeType}__{key.CollectionType}__{key.ContentType}",
        };

        if (!string.IsNullOrWhiteSpace(key.ParentContentType))
        {
            alternates.Add($"{shapeType}__{key.ParentContentType}__{key.CollectionType}");
            alternates.Add($"{shapeType}__{key.ParentContentType}__{key.ContentType}");
            alternates.Add($"{shapeType}__{key.ParentContentType}__{key.CollectionType}__{key.ContentType}");

            if (!string.IsNullOrWhiteSpace(key.NamedPart) && !key.NamedPart.Equals(key.CollectionType, StringComparison.Ordinal))
            {
                alternates.Add($"{shapeType}__{key.ParentContentType}__{key.NamedPart}");
                alternates.Add($"{shapeType}__{key.ParentContentType}__{key.NamedPart}__{key.ContentType}");
            }
        }

        return alternates.ToArray();
    }

    private readonly record struct ContentCardAlternatesCacheKey(
        string CollectionType,
        string ContentType,
        string ParentContentType,
        string NamedPart);
}
