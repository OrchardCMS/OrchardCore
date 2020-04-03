using OrchardCore.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Title.Models
{
    public class TitlePart : ContentPart
    {
        [Required]
        public string Title { get; set; }
    }
}
