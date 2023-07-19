using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Builders
{
    public static class ContentBuilderSettings
    {
        /// <summary>
        /// Replace current value, even for null values, union arrays.
        /// </summary>
        public static readonly JsonMergeSettings JsonMergeSettings = new()
        {
            MergeArrayHandling = MergeArrayHandling.Union,
            MergeNullValueHandling = MergeNullValueHandling.Merge,
        };

        /// <summary>
        /// A Json serializer that ignores properties which have their default values.
        /// To be able to have a default value : use [DefaultValue(true)] on a class property for example.
        /// </summary>
        public static readonly JsonSerializer IgnoreDefaultValuesSerializer = new()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };
    }
}
