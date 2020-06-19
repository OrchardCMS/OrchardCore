using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Media.Fields;

namespace OrchardCore.Media.ViewModels
{
    public class DisplayMediaFieldViewModel
    {
        public string[] Paths => Field.Paths;
        public MediaField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
