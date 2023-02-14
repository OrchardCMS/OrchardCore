using System;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Settings
{
    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class LocalizationSetContentPickerFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }
        public string[] DisplayedContentTypes { get; set; } = Array.Empty<string>();
    }
}
