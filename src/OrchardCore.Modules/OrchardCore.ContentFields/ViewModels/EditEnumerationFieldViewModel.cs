using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditEnumerationFieldViewModel
    {
        public string Value { get; set; }
        public List<SelectListItem> Options { get; set; }
        public EnumerationField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
