using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Forms.ViewModels
{
    public class ButtonPartEditViewModel
    {
        public string Name { get; set; }

        [Required]
        public string Text { get; set; }

        public string Type { get; set; }
    }
}
