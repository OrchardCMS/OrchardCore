using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.Metadata.Models
{
    public static class MetaDataExtensions
    {
        /// <summary>
        /// Populates an object with the values from a <see cref="JObject"/> instance.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="target"></param>
        public static void Populate(this JObject settings, object target)
        {
            JsonConvert.PopulateObject(settings.ToString(), target);
        }
    }
}
