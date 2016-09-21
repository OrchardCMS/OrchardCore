using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Title.Model
{
    public class TitlePart : ContentPart
    {
        [Required]
        public string Title { get; set; }
    }
}
