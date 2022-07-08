using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels
{
    public class HtmlMenuItemPartEditViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string Html { get; set; }

        [BindNever]
        public HtmlMenuItemPart MenuItemPart { get; set; }
    }
}
