using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class TimeField : ContentField
    {
        public TimeSpan? Value { get; set; }
    }
}
