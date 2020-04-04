using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Templates.Models
{
    public class TemplatesDocument : BaseDocument
    {
        public Dictionary<string, Template> Templates { get; } = new Dictionary<string, Template>(StringComparer.OrdinalIgnoreCase);
    }

    public class Template
    {
        public string Content { get; set; }
        public string Description { get; set; }
    }
}
