using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditKeyValuePairsFieldViewModel
    {
        // JSON serialized list of strings
        public string Keys { get; set; }

        // JSON serialized list of strings
        public string Values { get; set; }

        [BindNever]
        public KeyValuePairsField Field { get; set; }

        [BindNever]
        public ContentPart Part { get; set; }

        [BindNever]
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
