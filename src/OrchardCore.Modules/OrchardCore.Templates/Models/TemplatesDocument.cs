using System.Collections.Immutable;

namespace OrchardCore.Templates.Models
{
    public class TemplatesDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents

        public IImmutableDictionary<string, Template> Templates { get; set; } = ImmutableDictionary.Create<string, Template>();
    }

    public class Template
    {
        public string Content { get; set; }
        public string Description { get; set; }
    }
}
