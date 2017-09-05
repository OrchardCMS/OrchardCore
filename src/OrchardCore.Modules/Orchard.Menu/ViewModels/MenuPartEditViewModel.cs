using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels
{
    public class MenuPartEditViewModel
    {
        public string Hierarchy { get; set; }

        [BindNever]
        public MenuPart MenuPart { get; set; }
    }
}
