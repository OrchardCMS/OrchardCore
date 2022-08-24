using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class InputPart : ContentPart
    {
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }

        [DefaultValue(LabelOptions.None)]
        public LabelOptions LabelOption { get; set; }

        public string Label { get; set; }

        [DefaultValue(ValidationOptions.None)]
        public ValidationOptions ValidationOption { get; set; }
    }
}
