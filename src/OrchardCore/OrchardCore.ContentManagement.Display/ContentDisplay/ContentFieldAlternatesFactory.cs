using System.Collections.Concurrent;
using System.Collections.Frozen;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

/// <summary>
/// Provides cached alternate patterns for content fields.
/// Alternates are computed once per unique field configuration and cached for reuse.
/// </summary>
public static class ContentFieldAlternatesFactory
{
    private const string DisplayToken = "_Display";

    private static readonly ConcurrentDictionary<FieldAlternatesCacheKey, FieldAlternatesCollection> _cache = new();

    /// <summary>
    /// Gets or creates cached alternates for a content field configuration.
    /// </summary>
    /// <param name="typePartDefinition">The content type part definition.</param>
    /// <param name="partFieldDefinition">The content part field definition.</param>
    /// <param name="shapeType">The shape type being rendered.</param>
    /// <returns>A cached collection of alternates.</returns>
    public static FieldAlternatesCollection GetOrCreate(
        ContentTypePartDefinition typePartDefinition,
        ContentPartFieldDefinition partFieldDefinition,
        string shapeType)
    {
        var key = new FieldAlternatesCacheKey(
            typePartDefinition.ContentTypeDefinition.Name,
            typePartDefinition.PartDefinition.Name,
            typePartDefinition.Name,
            partFieldDefinition.FieldDefinition.Name,
            partFieldDefinition.Name,
            partFieldDefinition.Editor() ?? string.Empty,
            partFieldDefinition.DisplayMode() ?? string.Empty,
            shapeType);

        return _cache.GetOrAdd(key, static k => new FieldAlternatesCollection(k));
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
        string ShapeType);

    /// <summary>
    /// Pre-computed alternates collection for a field configuration.
    /// </summary>
    public sealed class FieldAlternatesCollection
    {
        private readonly FieldAlternatesCacheKey _key;
        private readonly ConcurrentDictionary<string, FrozenSet<string>> _alternatesByDisplayType = new(StringComparer.OrdinalIgnoreCase);

        // Pre-computed values
        private readonly string _editorFieldType;
        private readonly bool _hasDisplayMode;
        private readonly bool _isFieldTypeShape;
        private readonly bool _isEditorShape;

        internal FieldAlternatesCollection(FieldAlternatesCacheKey key)
        {
            _key = key;

            _editorFieldType = !string.IsNullOrEmpty(key.Editor)
                ? key.FieldType + "_Edit__" + key.Editor
                : key.FieldType + "_Edit";

            _hasDisplayMode = !string.IsNullOrEmpty(key.DisplayMode);
            _isFieldTypeShape = key.ShapeType == key.FieldType;
            _isEditorShape = key.ShapeType == _editorFieldType;
        }

        /// <summary>
        /// Gets whether the shape is an editor shape.
        /// </summary>
        public bool IsEditorShape => _isEditorShape;

        /// <summary>
        /// Gets whether the shape has a display mode configured.
        /// </summary>
        public bool HasDisplayMode => _hasDisplayMode;

        /// <summary>
        /// Gets the display mode for this field configuration.
        /// </summary>
        public string DisplayMode => _key.DisplayMode;

        /// <summary>
        /// Gets the field name.
        /// </summary>
        public string FieldName => _key.FieldName;

        /// <summary>
        /// Gets the field type.
        /// </summary>
        public string FieldType => _key.FieldType;

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
        /// Gets the editor field type.
        /// </summary>
        public string EditorFieldType => _editorFieldType;

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
            var displayTypes = new[] { string.Empty, "_" + displayType };

            // [ShapeType]_[DisplayType], e.g. TextField.Summary
            alternates.Add($"{_key.ShapeType}_{displayType}");

            if (_isFieldTypeShape)
            {
                foreach (var dt in displayTypes)
                {
                    // [PartType]__[FieldName], e.g. HtmlBodyPart-Description
                    alternates.Add($"{_key.PartType}{dt}__{_key.FieldName}");

                    // [ContentType]__[FieldType], e.g. Blog-TextField, LandingPage-TextField
                    alternates.Add($"{_key.ContentType}{dt}__{_key.FieldType}");

                    // [ContentType]__[PartName]__[FieldName], e.g. Blog-HtmlBodyPart-Description, LandingPage-Services-Description
                    alternates.Add($"{_key.ContentType}{dt}__{_key.PartType}__{_key.FieldName}");
                }
            }
            else
            {
                if (_hasDisplayMode)
                {
                    // [FieldType]_[DisplayType]__[DisplayMode]_Display, e.g. TextField-Header.Display.Summary
                    alternates.Add($"{_key.FieldType}_{displayType}__{_key.DisplayMode}{DisplayToken}");
                }

                for (var i = 0; i < displayTypes.Length; i++)
                {
                    var dt = displayTypes[i];
                    var shapeType = _key.ShapeType;

                    if (_hasDisplayMode)
                    {
                        shapeType = $"{_key.FieldType}__{_key.DisplayMode}";

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
                    alternates.Add($"{_key.FieldType}{dt}__{shapeType}");

                    // [PartType]__[FieldName]__[ShapeType], e.g. HtmlBodyPart-Description-TextFieldSummary
                    alternates.Add($"{_key.PartType}{dt}__{_key.FieldName}__{shapeType}");

                    // [ContentType]__[FieldType]__[ShapeType], e.g. Blog-TextField-TextFieldSummary, LandingPage-TextField-TextFieldSummary
                    alternates.Add($"{_key.ContentType}{dt}__{_key.FieldType}__{shapeType}");

                    // [ContentType]__[PartName]__[FieldName]__[ShapeType], e.g. Blog-HtmlBodyPart-Description-TextFieldSummary, LandingPage-Services-Description-TextFieldSummary
                    alternates.Add($"{_key.ContentType}{dt}__{_key.PartName}__{_key.FieldName}__{shapeType}");
                }
            }

            return alternates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
