using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Markdown.Fields;

namespace OrchardCore.Markdown.ViewModels
{
    public class EditMarkdownFieldViewModel
    {
        public string Markdown { get; set; }
        public MarkdownField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
