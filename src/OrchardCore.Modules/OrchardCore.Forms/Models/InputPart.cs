using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class InputPart : ContentPart
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
        public string Label { get; set; }
    }
}
