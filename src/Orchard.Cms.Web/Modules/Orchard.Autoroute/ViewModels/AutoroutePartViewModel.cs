using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;

namespace Orchard.Autoroute.ViewModels
{
    public class AutoroutePartViewModel
    {
        public string Path { get; set; }

        [BindNever]
        public AutoroutePart AutoroutePart { get; set; }

        [BindNever]
        public AutoroutePartSettings Settings { get; set; }
    }
}
