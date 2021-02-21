using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class ValidationRulePart : ContentPart
    {
        public bool IsNeedValidate { get; set; }
        public string Type { get; set; }
        public string Option { get; set; }

    }
}
