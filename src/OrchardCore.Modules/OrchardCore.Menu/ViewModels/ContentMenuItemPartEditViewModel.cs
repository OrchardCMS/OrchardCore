using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels;

public class ContentMenuItemPartEditViewModel
{
    public string Name { get; set; }

    public bool CheckContentPermissions { get; set; }

    [BindNever]
    public ContentMenuItemPart MenuItemPart { get; set; }
}
