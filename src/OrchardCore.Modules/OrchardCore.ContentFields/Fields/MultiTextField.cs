using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentFields.Fields
{
    public class MultiTextField : ContentField
    {
        public string[] Values { get; set; } = Array.Empty<string>();
    }
}
