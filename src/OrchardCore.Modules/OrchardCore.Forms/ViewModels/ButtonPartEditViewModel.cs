using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Forms.ViewModels
{
    public class ButtonPartEditViewModel
    {
        [Required]
        public string Text { get; set; }

        public string Type { get; set; }

        public bool ReCaptchaSettingsAreConfigured { get; set; }

        public bool ReCaptchaV3Protected { get; set; }

        public string FormId { get; set; }
    }
}
