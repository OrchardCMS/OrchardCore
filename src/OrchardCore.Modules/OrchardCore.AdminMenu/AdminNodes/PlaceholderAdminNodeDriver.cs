using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.AdminNodes;

public sealed class PlaceholderAdminNodeDriver : DisplayDriver<MenuItem, PlaceholderAdminNode>
{
    private readonly IPermissionService _permissionService;

    public PlaceholderAdminNodeDriver(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public override Task<IDisplayResult> DisplayAsync(PlaceholderAdminNode treeNode, BuildDisplayContext context)
    {
        return CombineAsync(
            View("PlaceholderAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
            View("PlaceholderAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(PlaceholderAdminNode treeNode, BuildEditorContext context)
    {
        return Initialize<PlaceholderAdminNodeViewModel>("PlaceholderAdminNode_Fields_TreeEdit", async model =>
        {
            model.LinkText = treeNode.LinkText;
            model.IconClass = treeNode.IconClass;

            var permissions = await _permissionService.GetPermissionsAsync();

            var selectedPermissions = await _permissionService.FindByNamesAsync(treeNode.PermissionNames);

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
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(PlaceholderAdminNode treeNode, UpdateEditorContext context)
    {
        var model = new PlaceholderAdminNodeViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            x => x.LinkText,
            x => x.IconClass,
            x => x.SelectedPermissionNames);

        treeNode.LinkText = model.LinkText;
        treeNode.IconClass = model.IconClass;

        var selectedPermissions =
            model.SelectedPermissionNames == null
            ? []
            : model.SelectedPermissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var permissions = await _permissionService.FindByNamesAsync(selectedPermissions);
        treeNode.PermissionNames = permissions.Select(p => p.Name).ToArray();

        return Edit(treeNode, context);
    }
}
