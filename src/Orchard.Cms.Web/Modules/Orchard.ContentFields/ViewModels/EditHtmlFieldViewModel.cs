using Orchard.ContentFields.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentFields.ViewModels
{
    public class EditHtmlFieldViewModel
    {
        public string Html { get; set; }
        public HtmlField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
