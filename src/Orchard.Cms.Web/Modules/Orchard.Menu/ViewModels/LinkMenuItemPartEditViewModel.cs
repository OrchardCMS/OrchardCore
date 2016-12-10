using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orchard.Menu.Models;

namespace Orchard.Menu.ViewModels
{
    public class LinkMenuItemPartEditViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        [BindNever]
        public LinkMenuItemPart MenuItemPart { get; set; }
    }
}
