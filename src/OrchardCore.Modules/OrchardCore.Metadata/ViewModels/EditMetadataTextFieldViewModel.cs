using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Metadata.Fields;

namespace OrchardCore.Metadata.ViewModels
{
    public class EditMetadataTextFieldViewModel
    {
        public string Value { get; set; }
        public MetadataTextField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
