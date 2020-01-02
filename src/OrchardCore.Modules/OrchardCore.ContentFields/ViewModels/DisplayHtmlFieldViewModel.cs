using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class DisplayHtmlFieldViewModel : IContent
    {
        public string Html { get; set; }
        public HtmlField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentItem ContentItem { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
