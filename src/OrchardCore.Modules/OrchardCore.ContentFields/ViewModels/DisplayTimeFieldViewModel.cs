using System;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class DisplayTimeFieldViewModel
    {
        public TimeSpan? Value => Field.Value;
        public TimeField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
