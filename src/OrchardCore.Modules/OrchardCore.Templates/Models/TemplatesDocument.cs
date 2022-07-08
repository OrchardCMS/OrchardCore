using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Templates.Models
{
    public class TemplatesDocument : Document
    {
        public Dictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>(StringComparer.OrdinalIgnoreCase);
    }

    public class Template
    {
        public string Content { get; set; }
        public string Description { get; set; }
    }
}
