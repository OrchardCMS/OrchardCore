using System.Globalization;

namespace OrchardCore.ContentManagement.Models
{
    public class ContentPickerAspect
    {
        public ContentPickerAspect(object contentPickerFieldSettings, CultureInfo culture)
        {
            ContentPickerFieldSettings = contentPickerFieldSettings;
            Culture = culture;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public object ContentPickerFieldSettings { get; }
        public CultureInfo Culture { get; }
    }
}
