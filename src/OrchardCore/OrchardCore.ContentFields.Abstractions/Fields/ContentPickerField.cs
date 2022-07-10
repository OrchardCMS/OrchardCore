using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class ContentPickerField : ContentField
    {
        public string[] ContentItemIds { get; set; } = Array.Empty<string>();
    }
}
