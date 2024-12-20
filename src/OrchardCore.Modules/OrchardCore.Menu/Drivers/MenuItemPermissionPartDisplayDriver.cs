using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Menu.Models;
using OrchardCore.Menu.ViewModels;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu.Drivers;

public sealed class MenuItemPermissionPartDisplayDriver : ContentPartDisplayDriver<MenuItemPermissionPart>
{
    private readonly IPermissionService _permissionService;

    public MenuItemPermissionPartDisplayDriver(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public override IDisplayResult Edit(MenuItemPermissionPart part, BuildPartEditorContext context)
    {
        return Initialize<MenuItemPermissionViewModel>("MenuItemPermissionPart_Edit", async model =>
        {
            var permissions = await _permissionService.GetPermissionsAsync();

            var selectedPermissions = permissions.Where(p => part.PermissionNames.Contains(p.Name));

            model.SelectedItems = selectedPermissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = p.Description
                }).ToArray();

            model.AllItems = permissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = p.Description
                }).ToArray();
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(MenuItemPermissionPart part, UpdatePartEditorContext context)
    {
        var model = new MenuItemPermissionViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            x => x.SelectedPermissionNames);

        var selectedPermissions = model.SelectedPermissionNames == null
            ? []
            : model.SelectedPermissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var permissions = await _permissionService.GetPermissionsAsync();
        part.PermissionNames = permissions
            .Where(p => selectedPermissions.Contains(p.Name))
            .Select(p => p.Name).ToArray();

        return Edit(part, context);
    }
}
