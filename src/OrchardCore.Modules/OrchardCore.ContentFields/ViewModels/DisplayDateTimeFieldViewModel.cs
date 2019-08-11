using System;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class DisplayDateTimeFieldViewModel
    {
        public DateTime? Value => Field.Value;
        public DateTime? LocalDateTime { get; set; }
        public DateTimeField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
