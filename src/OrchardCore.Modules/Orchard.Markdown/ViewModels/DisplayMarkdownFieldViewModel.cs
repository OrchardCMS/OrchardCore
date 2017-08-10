using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.Markdown.Fields;

namespace Orchard.Markdown.ViewModels
{
    public class MarkdownFieldViewModel
    {
        public string Markdown { get; set; }
        public string Html { get; set; }
        public MarkdownField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
