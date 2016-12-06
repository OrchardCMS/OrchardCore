using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Menu.Models;

namespace Orchard.Menu.ViewModels
{
    public class MenuPartEditViewModel
    {
        public string Hierarchy { get; set; }

        [BindNever]
        public MenuPart MenuPart { get; set; }
    }
}
