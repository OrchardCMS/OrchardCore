using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditContentPickerFieldViewModel
    {
        public string ContentItemIds { get; set; }
        public ContentPickerField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }

        [BindNever]
        public IList<VueMultiselectItemViewModel> SelectedItems { get; set; }
    }
}
