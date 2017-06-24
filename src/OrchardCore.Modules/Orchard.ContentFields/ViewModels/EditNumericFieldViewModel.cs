using Orchard.ContentFields.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentFields.ViewModels
{
    public class EditNumericFieldViewModel
    {
        public string Value { get; set; }
        public NumericField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
