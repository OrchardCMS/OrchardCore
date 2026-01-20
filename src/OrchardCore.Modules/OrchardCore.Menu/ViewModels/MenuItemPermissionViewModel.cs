using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Menu.ViewModels;

public class MenuItemPermissionViewModel
{
    public string SelectedPermissionNames { get; set; }

    [BindNever]
    public IList<PermissionViewModel> SelectedItems { get; set; }

    [BindNever]
    public IList<PermissionViewModel> AllItems { get; set; }
}
