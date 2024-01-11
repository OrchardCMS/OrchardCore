using System;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Media.Fields
{
    public static class MediaFieldAnchorExtensions
    {
        /// <summary>
        /// Anchors are a less well known property of a media field.
        /// </summary>
        public static Anchor[] GetAnchors(this MediaField mediaField)
        {
            var anchors = mediaField.Content["Anchors"] as JArray;

            return anchors != null ? anchors.ToObject<Anchor[]>() : Array.Empty<Anchor>();
        }

        /// <summary>
        /// Tags names are a less well known property of a media field.
        /// </summary>
        public static void SetAnchors(this MediaField mediaField, Anchor[] anchors)
        {
            mediaField.Content["Anchors"] = JArray.FromObject(anchors);
        }

    }
}
