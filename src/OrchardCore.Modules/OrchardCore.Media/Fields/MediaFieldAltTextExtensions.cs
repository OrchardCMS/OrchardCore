using System;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp;

namespace OrchardCore.Media.Fields
{
    public static class MediaFieldAltTextExtensions
    {
        /// <summary>
        /// Alt text are a less well known property of a media field.
        /// </summary>
        public static string[] GetAltTexts(this MediaField mediaField)
        {
            var altTexts = mediaField.Content["AltTexts"] as JArray;

            return altTexts != null ? altTexts.ToObject<string[]>() : Array.Empty<string>();
        }

        /// <summary>
        /// Tags names are a less well known property of a media field.
        /// </summary>
        public static void SetAltTexts(this MediaField mediaField, string[] altTexts)
        {
            mediaField.Content["AltTexts"] = JArray.FromObject(altTexts);
        }

        /// <summary>
        /// Centers are a less well known property of a media field.
        /// </summary>
        public static float?[][] GetCenters(this MediaField mediaField)
        {
            var centers = mediaField.Content["Centers"] as JArray;

            return centers != null ? centers.ToObject<float?[][]>() : Array.Empty<float?[]>();
        }

        /// <summary>
        /// Tags names are a less well known property of a media field.
        /// </summary>
        public static void SetCenters(this MediaField mediaField, float?[][] centers)
        {
            mediaField.Content["Centers"] = JArray.FromObject(centers);
        }

    }
}
