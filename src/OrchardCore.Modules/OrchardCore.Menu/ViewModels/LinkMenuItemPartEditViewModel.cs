using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels
{
    public class LinkMenuItemPartEditViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Target { get; set; } = "_self";

        [BindNever]
        public LinkMenuItemPart MenuItemPart { get; set; }
    }
}
