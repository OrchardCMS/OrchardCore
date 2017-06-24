using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orchard.ContentManagement.Metadata.Builders
{
    public static class ContentBuilderSettings
    {
        /// <summary>
        /// Replace current value, even for null values, union arrays.
        /// </summary>
        public static readonly JsonMergeSettings JsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union, MergeNullValueHandling = MergeNullValueHandling.Merge };
    }
}
