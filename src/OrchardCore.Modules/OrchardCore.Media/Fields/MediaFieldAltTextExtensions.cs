using System;
using Newtonsoft.Json.Linq;

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
    }
}
