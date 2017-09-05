using System.Collections.Generic;

namespace OrchardCore.Templates.Models
{
    public class TemplatesDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public Dictionary<string, Template> Templates { get; } = new Dictionary<string, Template>();
    }

    public class Template
    {
        public string Content { get; set; }
        public string Description { get; set; }
    }
}
