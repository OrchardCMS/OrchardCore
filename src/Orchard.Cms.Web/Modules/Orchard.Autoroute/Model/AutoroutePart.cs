using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Autoroute.Model
{
    public class AutoroutePart : ContentPart
    {
        public string Path { get; set; }
    }
}
