using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class ButtonPart : ContentPart
    {
        public string Text { get; set; }
        public string Type { get; set; }
        public bool ReCaptchaV3Protected { get; set; }
        public string SiteKey { get; set; }
        public string FormId { get; set; }
    }
}
