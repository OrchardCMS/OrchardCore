using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Templates.Models
{
    public class TemplatesDocument
    {
        public Dictionary<string, Template> Templates { get; } = new Dictionary<string, Template>(StringComparer.OrdinalIgnoreCase);
    }

    public class Template
    {
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

        public string Content { get; set; }
        public string Description { get; set; }
    }
}
