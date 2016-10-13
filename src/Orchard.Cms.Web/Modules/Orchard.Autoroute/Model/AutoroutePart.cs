using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Autoroute.Model
{
    public class AutoroutePart : ContentPart
    {
        [Required]
        public string Path { get; set; }
    }
}
