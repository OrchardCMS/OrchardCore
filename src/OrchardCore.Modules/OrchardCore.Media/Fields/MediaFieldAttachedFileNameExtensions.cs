using System;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Media.Fields
{
    public static class MediaFieldAttachedFileNameExtensions
    {
        /// <summary>
        /// Gets the names of <see cref="MediaField"/> attached files.
        /// </summary>
        public static string[] GetAttachedFileNames(this MediaField mediaField)
        {
            var filenames = mediaField.Content["AttachedFileNames"] as JArray;

            return filenames != null ? filenames.ToObject<string[]>() : Array.Empty<string>(); ;
        }

        /// <summary>
        /// Sets the names of <see cref="MediaField"/> attached files.
        /// </summary>
        public static void SetAttachedFileNames(this MediaField mediaField, string[] filenames)
        {
            mediaField.Content["AttachedFileNames"] = JArray.FromObject(filenames);
        }

    }
}
