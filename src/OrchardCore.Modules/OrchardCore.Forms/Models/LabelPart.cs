using System.ComponentModel.DataAnnotations;
using OrchardCore.ContentManagement;

namespace OrchardCore.Forms.Models
{
    public class LabelPart : ContentPart
    {
        [Required]
        public string Text { get; set; }
        public string For { get; set; }
    }
}
