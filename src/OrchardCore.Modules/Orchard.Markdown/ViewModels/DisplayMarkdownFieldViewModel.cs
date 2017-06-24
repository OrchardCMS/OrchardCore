using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.Markdown.Fields;

namespace Orchard.Markdown.ViewModels
{
    public class DisplayMarkdownFieldViewModel
    {
        public MarkdownField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
