using System;

namespace OrchardCore.ContentFields.Settings
{
    public class ContentPickerFieldSettings
    {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public bool Multiple { get; set; }
        public bool DisplayAllContentTypes { get; set; }
        public string[] DisplayedContentTypes { get; set; } = Array.Empty<string>();
        public string[] DisplayedStereotypes { get; set; } = Array.Empty<string>();
    }
}
