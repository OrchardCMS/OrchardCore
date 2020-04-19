using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels
{
    public class ContentPickerMenuItemPartEditViewModel
    {
        public string Name { get; set; }

        [BindNever]
        public ContentPickerMenuItemPart MenuItemPart { get; set; }
    }
}
