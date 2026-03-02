using System.Collections.Concurrent;
using System.Collections.Frozen;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Provides cached alternate patterns for content parts.
/// Alternates are computed once per unique part configuration and cached for reuse.
/// </summary>
public static class ContentPartAlternatesFactory
{
    private const string DisplayToken = "_Display";

    private static readonly ConcurrentDictionary<PartAlternatesCacheKey, PartAlternatesCollection> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a content part configuration.
    /// </summary>
    /// <param name="definition">The content type part definition.</param>
    /// <param name="shapeType">The shape type being rendered.</param>
    /// <returns>A cached collection of alternates.</returns>
    public static PartAlternatesCollection GetOrCreate(ContentTypePartDefinition definition, string shapeType)
    {
        var key = new PartAlternatesCacheKey(
            definition.ContentTypeDefinition.Name,
            definition.PartDefinition.Name,
            definition.Name,
            definition.Editor() ?? string.Empty,
            definition.DisplayMode() ?? string.Empty,
            definition.ContentTypeDefinition.GetStereotype() ?? string.Empty,
            shapeType);

        return _cache.GetOrAdd(key, static k => new PartAlternatesCollection(k));
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
        string ShapeType);

    /// <summary>
    /// Pre-computed alternates collection for a part configuration.
    /// </summary>
    public sealed class PartAlternatesCollection
    {
        private readonly PartAlternatesCacheKey _key;
        private readonly ConcurrentDictionary<string, FrozenSet<string>> _alternatesByDisplayType = new(StringComparer.OrdinalIgnoreCase);

        // Pre-computed values
        private readonly string _editorPartType;
        private readonly bool _hasDisplayMode;
        private readonly bool _isDisplayModeShapeType;
        private readonly bool _isPartTypeShape;
        private readonly bool _isEditorShape;
        private readonly bool _hasStereotype;
        private readonly bool _partTypeEqualsPartName;

        internal PartAlternatesCollection(PartAlternatesCacheKey key)
        {
            _key = key;

            _editorPartType = !string.IsNullOrEmpty(key.Editor)
                ? key.PartType + "_Edit__" + key.Editor
                : key.PartType + "_Edit";

            _hasDisplayMode = !string.IsNullOrEmpty(key.DisplayMode);
            _isDisplayModeShapeType = key.ShapeType == key.PartType + "_Display__" + key.DisplayMode;
            _isPartTypeShape = key.ShapeType == key.PartType;
            _isEditorShape = key.ShapeType == _editorPartType;
            _hasStereotype = !string.IsNullOrEmpty(key.Stereotype);
            _partTypeEqualsPartName = key.PartType == key.PartName;
        }

        /// <summary>
        /// Gets whether the shape type is a display mode shape type.
        /// </summary>
        public bool IsDisplayModeShapeType => _isDisplayModeShapeType;

        /// <summary>
        /// Gets whether the shape has a display mode configured.
        /// </summary>
        public bool HasDisplayMode => _hasDisplayMode;

        /// <summary>
        /// Gets the display mode for this part configuration.
        /// </summary>
        public string DisplayMode => _key.DisplayMode;

        /// <summary>
        /// Gets the part name.
        /// </summary>
        public string PartName => _key.PartName;

        /// <summary>
        /// Gets the part type.
        /// </summary>
        public string PartType => _key.PartType;

        /// <summary>
        /// Gets the content type.
        /// </summary>
        public string ContentType => _key.ContentType;

        /// <summary>
        /// Gets the editor part type.
        /// </summary>
        public string EditorPartType => _editorPartType;

        /// <summary>
        /// Gets whether this is an editor shape.
        /// </summary>
        public bool IsEditorShape => _isEditorShape;

        /// <summary>
        /// Gets the cached alternates for a specific display type.
        /// </summary>
        /// <param name="displayType">The display type (e.g., "Summary", "Detail").</param>
        /// <returns>A frozen set of alternates.</returns>
        public FrozenSet<string> GetAlternates(string displayType)
        {
            return _alternatesByDisplayType.GetOrAdd(displayType, BuildAlternates);
        }

        private FrozenSet<string> BuildAlternates(string displayType)
        {
            var alternates = new List<string>();

            string[] displayTypes;

            if (_isEditorShape)
            {
                displayTypes = ["_" + displayType];
            }
            else
            {
                displayTypes = ["", "_" + displayType];

                if (!_isDisplayModeShapeType)
                {
                    // [ShapeType]_[DisplayType], e.g. HtmlBodyPart.Summary, BagPart.Summary, ListPartFeed.Summary
                    alternates.Add($"{_key.ShapeType}_{displayType}");
                }
            }

            if (_isPartTypeShape || _isEditorShape)
            {
                foreach (var dt in displayTypes)
                {
                    // [ContentType]_[DisplayType]__[PartType], e.g. Blog-HtmlBodyPart, LandingPage-BagPart
                    alternates.Add($"{_key.ContentType}{dt}__{_key.PartType}");

                    if (_hasStereotype)
                    {
                        // [Stereotype]__[DisplayType]__[PartType], e.g. Widget-ContentsMetadata
                        alternates.Add($"{_key.Stereotype}{dt}__{_key.PartType}");
                    }
                }

                if (!_partTypeEqualsPartName)
                {
                    foreach (var dt in displayTypes)
                    {
                        // [ContentType]_[DisplayType]__[PartName], e.g. LandingPage-Services
                        alternates.Add($"{_key.ContentType}{dt}__{_key.PartName}");

                        if (_hasStereotype)
                        {
                            // [Stereotype]_[DisplayType]__[PartType]__[PartName], e.g. Widget-ServicePart-Services
                            alternates.Add($"{_key.Stereotype}{dt}__{_key.PartType}__{_key.PartName}");
                        }
                    }
                }
            }
            else
            {
                if (_hasDisplayMode)
                {
                    // [PartType]_[DisplayType]__[DisplayMode]_Display, e.g. HtmlBodyPart-MyDisplayMode.Display.Summary
                    alternates.Add($"{_key.PartType}_{displayType}__{_key.DisplayMode}{DisplayToken}");
                }

                var lastAlternatesOfNamedPart = new List<string>();

                foreach (var dt in displayTypes)
                {
                    var shapeTypeSuffix = _key.ShapeType;
                    var displayTypeDisplayToken = dt;

                    if (_hasDisplayMode)
                    {
                        if (_isDisplayModeShapeType)
                        {
                            shapeTypeSuffix = _key.DisplayMode;
                        }
                        else
                        {
                            shapeTypeSuffix = $"{shapeTypeSuffix}__{_key.DisplayMode}";
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
                    alternates.Add($"{_key.ContentType}{displayTypeDisplayToken}__{_key.PartType}__{shapeTypeSuffix}");

                    if (_hasStereotype)
                    {
                        // [Stereotype]_[DisplayType]__[PartType]__[ShapeType], e.g. Blog-ListPart-ListPartFeed
                        alternates.Add($"{_key.Stereotype}{displayTypeDisplayToken}__{_key.PartType}__{shapeTypeSuffix}");
                    }

                    if (!_partTypeEqualsPartName)
                    {
                        // [ContentType]_[DisplayType]__[PartName]__[ShapeType], e.g. LandingPage-Services-BagPartSummary
                        lastAlternatesOfNamedPart.Add($"{_key.ContentType}{displayTypeDisplayToken}__{_key.PartName}__{shapeTypeSuffix}");

                        if (_hasStereotype)
                        {
                            // [Stereotype]_[DisplayType]__[PartType]__[PartName]__[ShapeType], e.g. Widget-ServicePart-Services-BagPartSummary
                            lastAlternatesOfNamedPart.Add($"{_key.Stereotype}{displayTypeDisplayToken}__{_key.PartType}__{_key.PartName}__{shapeTypeSuffix}");
                        }
                    }
                }

                alternates.AddRange(lastAlternatesOfNamedPart);
            }

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
