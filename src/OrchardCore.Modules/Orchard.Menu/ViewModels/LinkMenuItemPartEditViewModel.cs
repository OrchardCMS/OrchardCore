using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels
{
    public class LinkMenuItemPartEditViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        [BindNever]
        public LinkMenuItemPart MenuItemPart { get; set; }
    }
}
