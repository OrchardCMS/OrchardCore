using Orchard.ContentFields.Fields;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Metadata.Models;

namespace Orchard.ContentFields.ViewModels
{
    public class EditEnumerationFieldViewModel
    {
        public string Value { get; set; }
        public string[] SelectedValues
        {
            get
            {
                if (Field != null)
                {
                    return Field.SelectedValues;
                }

                return null;
            }
            set
            {
                if (Field != null)
                {
                    Field.SelectedValues = SelectedValues;
                }
            }
        }
        public EnumerationField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
