using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class UserPickerField : ContentField
    {
        public string[] UserIds { get; set; } = Array.Empty<string>();
    }
}
