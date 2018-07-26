using System.Collections.Generic;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditEnumerationFieldViewModel
    {
        public string Value { get; set; }
        public string[] Values { get; set; }
        public List<OptionViewModel> Options { get; set; }
        public EnumerationField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
    public class OptionViewModel
    {
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
