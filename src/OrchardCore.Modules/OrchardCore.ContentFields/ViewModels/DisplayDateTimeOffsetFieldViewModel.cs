using System;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class DisplayDateTimeOffsetFieldViewModel
    {
        public DateTime? Value => Field.Value;
        public DateTime? LocalDateTime { get; set; }
        public DateTimeOffsetField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
