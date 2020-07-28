using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.Shortcodes.Models
{
    public class ShortcodeTemplatesDocument
    {
        public Dictionary<string, ShortcodeTemplate> ShortcodeTemplates { get; } = new Dictionary<string, ShortcodeTemplate>(StringComparer.OrdinalIgnoreCase);
    }

    public class ShortcodeTemplate
    {
        /// <summary>
        /// True if the object can't be used to update the database.
        /// </summary>
        [JsonIgnore]
        public bool IsReadonly { get; set; }

        public string Content { get; set; }
        public string Hint { get; set; }
        public string Usage { get; set; }
        public string DefaultShortcode { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
    }
}
