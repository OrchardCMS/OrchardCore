using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class DateTimeField : ContentField
    {
        public DateTime? Value { get; set; }
    }
}
