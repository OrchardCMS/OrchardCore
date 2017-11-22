using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Autoroute.Model;
using OrchardCore.Autoroute.Models;

namespace OrchardCore.Autoroute.ViewModels
{
    public class AutoroutePartViewModel
    {
        public string Path { get; set; }
        public bool SetHomepage { get; set; }
        public bool IsHomepage { get; set; }
        
        [BindNever]
        public AutoroutePart AutoroutePart { get; set; }

        [BindNever]
        public AutoroutePartSettings Settings { get; set; }
    }
}
