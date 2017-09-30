using Orchard.ContentFields.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentFields.ViewModels
{
    public class DisplayLinkFieldViewModel
    {
        public LinkField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
