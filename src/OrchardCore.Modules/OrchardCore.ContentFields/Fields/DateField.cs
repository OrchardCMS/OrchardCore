using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class DateField : ContentField
    {
        public DateTime? Value { get; set; }
    }
}
