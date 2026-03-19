using System.Collections.Concurrent;

namespace OrchardCore.Contents;

/// <summary>
/// Provides cached alternate patterns for ContentsMetadata shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentsMetadataAlternatesFactory
{
    private static readonly ConcurrentDictionary<MetadataAlternatesCacheKey, string[]> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a ContentsMetadata shape configuration.
    /// </summary>
    public static string[] GetAlternates(string stereotype, string displayType)
    {
        var key = new MetadataAlternatesCacheKey(stereotype ?? string.Empty, displayType ?? string.Empty);
        return _cache.GetOrAdd(key, BuildAlternates);
    }

    internal readonly record struct MetadataAlternatesCacheKey(
        string Stereotype,
        string DisplayType);

    private static string[] BuildAlternates(MetadataAlternatesCacheKey key)
    {
        var alternates = new List<string>();
        var hasStereotype = !string.IsNullOrEmpty(key.Stereotype) && !string.Equals("Content", key.Stereotype, StringComparison.OrdinalIgnoreCase);

        if (hasStereotype)
        {
            // [Stereotype]__ContentsMetadata
            alternates.Add($"{key.Stereotype}__ContentsMetadata");
        }

        if (!string.IsNullOrEmpty(key.DisplayType) && key.DisplayType != "Detail")
        {
            // ContentsMetadata_[DisplayType] e.g. ContentsMetadata_Summary
            alternates.Add($"ContentsMetadata_{key.DisplayType}");

            if (hasStereotype)
            {
                // [Stereotype]_[DisplayType]__ContentsMetadata e.g. Widget_Summary__ContentsMetadata
                alternates.Add($"{key.Stereotype}_{key.DisplayType}__ContentsMetadata");
            }
        }

        return alternates.ToArray();
    }
}
