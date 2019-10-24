using System;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class EditDateTimeOffsetFieldViewModel
    {
        public DateTime? LocalDateTime { get; set; }

        public string TimeZone { get; set; }

        public string CountryCode { get; set; }

        public long Offset { get; set; }

        public DateTimeOffsetField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
