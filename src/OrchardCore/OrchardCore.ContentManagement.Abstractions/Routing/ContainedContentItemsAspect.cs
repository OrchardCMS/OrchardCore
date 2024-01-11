using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Routing
{
    public class ContainedContentItemsAspect
    {
        /// <summary>
        /// Json accessors to provide a list of contained content items.
        /// </summary>
        public IList<Func<JObject, JArray>> Accessors { get; set; } = new List<Func<JObject, JArray>>();
    }
}
