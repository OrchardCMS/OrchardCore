using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Forms.ViewModels
{
    public class ButtonPartEditViewModel
    {
        [Required]
        public string Text { get; set; }

        public string Type { get; set; }
    }
}
