using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditMultiTextFieldViewModel
    {
        public string[] Values { get; set; } = Array.Empty<string>();

        [BindNever]
        public MultiTextField Field { get; set; }

        [BindNever]
        public ContentPart Part { get; set; }

        [BindNever]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
