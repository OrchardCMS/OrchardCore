using System;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Fields
{
    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class LocalizationSetContentPickerField : ContentField
    {
        public string[] LocalizationSets { get; set; } = Array.Empty<string>();
    }
}
