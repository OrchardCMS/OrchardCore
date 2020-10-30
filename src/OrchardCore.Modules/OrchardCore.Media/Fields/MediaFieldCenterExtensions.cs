using System;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;

namespace OrchardCore.Media.Fields
{
    public static class MediaFieldCenterExtensions
    {
        /// <summary>
        /// Centers are a less well known property of a media field.
        /// </summary>
        public static Center[] GetCenters(this MediaField mediaField)
        {
            var centers = mediaField.Content["Centers"] as JArray;

            return centers != null ? centers.ToObject<Center[]>() : Array.Empty<Center>();
        }

        /// <summary>
        /// Tags names are a less well known property of a media field.
        /// </summary>
        public static void SetCenters(this MediaField mediaField, Center[] centers)
        {
            mediaField.Content["Centers"] = JArray.FromObject(centers);
        }

    }
}
