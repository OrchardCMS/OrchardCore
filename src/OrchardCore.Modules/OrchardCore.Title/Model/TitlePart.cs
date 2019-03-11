using OrchardCore.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Title.Model
{
    public class TitlePart : ContentPart
    {
        public TitlePart()
        {
                
        }

        [Required]
        public string Title { get; set; }
    }
}
