using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class InputPart : ContentPart
    {
        public string Type { get; set; }
        public string DefaultValue { get; set; }
        public string Placeholder { get; set; }
    }
}
