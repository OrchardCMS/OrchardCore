using System;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Media.Fields
{
    public static class MediaFieldFilenameExtensions
    {
        /// <summary>
        /// Gets the filenames of <see cref="MediaField"/> items.
        /// </summary>
        public static string[] GetFilenames(this MediaField mediaField)
        {
            var filenames = mediaField.Content["Filename"] as JArray;

            return filenames != null ? filenames.ToObject<string[]>() : Array.Empty<string>(); ;
        }

        /// <summary>
        /// Sets the filenames of <see cref="MediaField"/> items.
        /// </summary>
        public static void SetFilenames(this MediaField mediaField, string[] filenames)
        {
            mediaField.Content["Filename"] = JArray.FromObject(filenames);
        }

    }
}
