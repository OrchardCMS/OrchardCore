using OrchardCore.AdminMenu.Services;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.AdminNodes;

public sealed class PlaceholderAdminNodeDriver : DisplayDriver<MenuItem, PlaceholderAdminNode>
{
    private readonly IAdminMenuPermissionService _adminMenuPermissionService;

    public PlaceholderAdminNodeDriver(IAdminMenuPermissionService adminMenuPermissionService)
    {
        _adminMenuPermissionService = adminMenuPermissionService;
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

            var permissions = await _adminMenuPermissionService.GetPermissionsAsync();

            var selectedPermissions = permissions.Where(p => treeNode.PermissionNames.Contains(p.Name));

            model.SelectedItems = selectedPermissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = p.Description
                }).ToList();
            model.AllItems = permissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = p.Description
                }).ToList();
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

        var selectedPermissions = (model.SelectedPermissionNames == null ? Array.Empty<string>() : model.SelectedPermissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries));
        var permissions = await _adminMenuPermissionService.GetPermissionsAsync();
        treeNode.PermissionNames = permissions
            .Where(p => selectedPermissions.Contains(p.Name))
            .Select(p => p.Name).ToArray();

        return Edit(treeNode, context);
    }
}
