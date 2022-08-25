using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels
{
    public class TextAreaPartEditViewModel
    {
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public LabelOptions LabelOption { get; set; }
        public string Label { get; set; }
        public ValidationOptions ValidationOption { get; set; }
    }
}
