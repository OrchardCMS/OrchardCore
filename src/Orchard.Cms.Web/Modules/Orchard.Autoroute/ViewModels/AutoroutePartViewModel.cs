using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Autoroute.Model;

namespace Orchard.Autoroute.ViewModels
{
    public class AutoroutePartViewModel
    {
        public string Path { get; set; }

        [BindNever]
        public AutoroutePart AutoroutePart { get; set; }
    }
}
