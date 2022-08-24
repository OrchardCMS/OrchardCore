using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels
{
    public class InputPartEditViewModel
    {
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public LabelOptions LabelOption { get; set; }
        public string Label { get; set; }
        public ValidationOptions ValidationOption { get; set; }
    }
}
