using System.Collections.Generic;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.ContentFields.ViewModels
{
    public class DisplayContentPickerFieldViewModel
    {
        public ContentPickerField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
