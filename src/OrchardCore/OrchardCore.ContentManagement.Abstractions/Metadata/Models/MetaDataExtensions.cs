using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public static class MetaDataExtensions
    {
        /// <summary>
        /// Populates an object with the values from a <see cref="JObject"/> instance.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="target"></param>
        [Obsolete("Migrate to PopulateSettings<T>. This will be removed in a future version.")]
        public static void Populate(this JObject settings, object target)
        {
            JsonConvert.PopulateObject(settings.ToString(), target);
        }

        [Obsolete("Migrate to PopulateSettings<T>. This will be removed in a future version.")]
        public static void Populate<T>(this JObject settings, object target)
        {
            var property = settings[typeof(T).Name];

            if (property != null)
            {
                JsonConvert.PopulateObject(property.ToString(), target);
            }
        }
    }
}
