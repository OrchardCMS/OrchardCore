using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Shortcodes.Models
{
    public class ShortcodeTemplatesDocument : Document
    {
        public Dictionary<string, ShortcodeTemplate> ShortcodeTemplates { get; } = new Dictionary<string, ShortcodeTemplate>(StringComparer.OrdinalIgnoreCase);
    }

    public class ShortcodeTemplate
    {
        public string Content { get; set; }
        public string Hint { get; set; }
        public string Usage { get; set; }
        public string DefaultValue { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();
    }
}
