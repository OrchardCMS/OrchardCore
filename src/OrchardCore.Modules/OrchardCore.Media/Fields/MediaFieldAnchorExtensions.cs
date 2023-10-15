using System.Text.Json;

namespace OrchardCore.Media.Fields
{
    public static class MediaFieldAnchorExtensions
    {
        private const string PropertyName = "Anchors";

        /// <summary>
        /// Anchors are a less well known property of a media field.
        /// </summary>
        public static Anchor[] GetAnchors(this MediaField mediaField) =>
            mediaField.GetArrayProperty<Anchor>(PropertyName);

        /// <summary>
        /// Tags names are a less well known property of a media field.
        /// </summary>
        public static void SetAnchors(this MediaField mediaField, Anchor[] anchors) =>
            mediaField.Content[PropertyName] = JsonSerializer.SerializeToNode(anchors);
    }
}
