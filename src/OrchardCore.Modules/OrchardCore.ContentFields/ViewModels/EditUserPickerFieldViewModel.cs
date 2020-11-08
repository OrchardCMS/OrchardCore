using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditUserPickerFieldViewModel
    {
        public string UserIds { get; set; }
        public UserPickerField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }

        [BindNever]
        public IList<VueMultiselectUserViewModel> SelectedUsers { get; set; } = new List<VueMultiselectUserViewModel>();
    }

    public class VueMultiselectUserViewModel
    {
        public string Id { get; set; }
        public string DisplayText { get; set; }
        public bool IsEnabled { get; set; }
    }
}
