using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.AdminNodes;

public sealed class LinkAdminNodeDriver : DisplayDriver<MenuItem, LinkAdminNode>
{
    private readonly IPermissionService _permissionService;

    public LinkAdminNodeDriver(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public override Task<IDisplayResult> DisplayAsync(LinkAdminNode treeNode, BuildDisplayContext context)
    {
        return CombineAsync(
            View("LinkAdminNode_Fields_TreeSummary", treeNode).Location("TreeSummary", "Content"),
            View("LinkAdminNode_Fields_TreeThumbnail", treeNode).Location("TreeThumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(LinkAdminNode treeNode, BuildEditorContext context)
    {
        return Initialize<LinkAdminNodeViewModel>("LinkAdminNode_Fields_TreeEdit", async model =>
        {
            model.LinkText = treeNode.LinkText;
            model.LinkUrl = treeNode.LinkUrl;
            model.IconClass = treeNode.IconClass;
            model.Target = treeNode.Target;

            var selectedPermissions = await _permissionService.FindByNamesAsync(treeNode.PermissionNames).ConfigureAwait(false);

            model.SelectedItems = selectedPermissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = p.Description,
                }).ToArray();

            var permissions = await _permissionService.GetPermissionsAsync().ConfigureAwait(false);

            model.AllItems = permissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = p.Description,
                }).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(LinkAdminNode treeNode, UpdateEditorContext context)
    {
        var model = new LinkAdminNodeViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            x => x.LinkUrl,
            x => x.LinkText,
            x => x.Target,
            x => x.IconClass,
            x => x.SelectedPermissionNames).ConfigureAwait(false);

        treeNode.LinkText = model.LinkText;
        treeNode.LinkUrl = model.LinkUrl;
        treeNode.Target = model.Target;
        treeNode.IconClass = model.IconClass;

        var selectedPermissions = model.SelectedPermissionNames == null
            ? []
            : model.SelectedPermissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var permissions = await _permissionService.FindByNamesAsync(selectedPermissions).ConfigureAwait(false);
        treeNode.PermissionNames = permissions.Select(p => p.Name).ToArray();

        return Edit(treeNode, context);
    }
}
