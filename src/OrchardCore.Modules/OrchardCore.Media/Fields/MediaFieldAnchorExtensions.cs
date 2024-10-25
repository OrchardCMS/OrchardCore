using System.Text.Json.Nodes;

namespace OrchardCore.Media.Fields;

public static class MediaFieldAnchorExtensions
{
    /// <summary>
    /// Anchors are a less well known property of a media field.
    /// </summary>
    public static Anchor[] GetAnchors(this MediaField mediaField)
    {
        var anchors = (JsonArray)mediaField.Content["Anchors"];

        return anchors is not null ? anchors.ToObject<Anchor[]>() : [];
    }

    /// <summary>
    /// Tags names are a less well known property of a media field.
    /// </summary>
    public static void SetAnchors(this MediaField mediaField, Anchor[] anchors)
    {
        mediaField.Content["Anchors"] = JArray.FromObject(anchors);
    }

}
