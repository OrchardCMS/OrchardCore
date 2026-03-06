using System.Collections.Concurrent;

namespace OrchardCore.Contents;

/// <summary>
/// Provides cached alternate patterns for ContentsMetadata shapes.
/// Alternates are computed once per unique configuration and cached for reuse.
/// </summary>
internal static class ContentsMetadataAlternatesFactory
{
    private static readonly ConcurrentDictionary<MetadataAlternatesCacheKey, MetadataAlternatesCollection> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a ContentsMetadata shape configuration.
    /// </summary>
    public static MetadataAlternatesCollection GetAlternates(string contentType, string stereotype)
    {
        var key = new MetadataAlternatesCacheKey(contentType, stereotype ?? string.Empty);
        return _cache.GetOrAdd(key, static k => new MetadataAlternatesCollection(k));
    }

    internal readonly record struct MetadataAlternatesCacheKey(
        string ContentType,
        string Stereotype);

    /// <summary>
    /// Pre-computed alternates collection for a ContentsMetadata shape configuration.
    /// </summary>
    internal sealed class MetadataAlternatesCollection
    {
        private readonly MetadataAlternatesCacheKey _key;
        private readonly ConcurrentDictionary<string, string[]> _alternatesByDisplayType = new(StringComparer.OrdinalIgnoreCase);
        private readonly bool _hasStereotype;

        internal MetadataAlternatesCollection(MetadataAlternatesCacheKey key)
        {
            _key = key;
            _hasStereotype = !string.IsNullOrEmpty(key.Stereotype) && !string.Equals("Content", key.Stereotype, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the cached alternates for a specific display type.
        /// </summary>
        public string[] GetAlternates(string displayType)
        {
            return _alternatesByDisplayType.GetOrAdd(displayType, BuildAlternates);
        }

        private string[] BuildAlternates(string displayType)
        {
            var alternates = new List<string>();

            if (_hasStereotype)
            {
                // [Stereotype]__ContentsMetadata
                alternates.Add($"{_key.Stereotype}__ContentsMetadata");
            }

            if (!string.IsNullOrEmpty(displayType) && displayType != "Detail")
            {
                // ContentsMetadata_[DisplayType] e.g. ContentsMetadata_Summary
                alternates.Add($"ContentsMetadata_{displayType}");

                if (_hasStereotype)
                {
                    // [Stereotype]_[DisplayType]__ContentsMetadata e.g. Widget_Summary__ContentsMetadata
                    alternates.Add($"{_key.Stereotype}_{displayType}__ContentsMetadata");
                }
            }

            return alternates.ToArray();
        }
    }
}
