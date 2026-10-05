using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.AdminNodes;

public sealed class LinkAdminNodeDriver : DisplayDriver<MenuItem, LinkAdminNode>
{
    private readonly IPermissionService _permissionService;
    private readonly IStringLocalizerFactory _stringLocalizerFactory;

    public LinkAdminNodeDriver(
        IPermissionService permissionService,
        IStringLocalizerFactory stringLocalizerFactory)
    {
        _permissionService = permissionService;
        _stringLocalizerFactory = stringLocalizerFactory;
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
        return Initialize<LinkAdminNodeViewModel, LinkAdminNode, IPermissionService, IStringLocalizerFactory>("LinkAdminNode_Fields_TreeEdit", static async (model, treeNode, permissionService, stringLocalizerFactory) =>
        {
            model.LinkText = treeNode.LinkText;
            model.LinkUrl = treeNode.LinkUrl;
            model.IconClass = treeNode.IconClass;
            model.Target = treeNode.Target;

            var selectedPermissions = await permissionService.FindByNamesAsync(treeNode.PermissionNames);

            model.SelectedItems = selectedPermissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = stringLocalizerFactory.Localize(p.Description)?.Value,
                }).ToArray();

            var permissions = await permissionService.GetPermissionsAsync();

            model.AllItems = permissions
                .Select(p => new PermissionViewModel
                {
                    Name = p.Name,
                    DisplayText = stringLocalizerFactory.Localize(p.Description)?.Value,
                }).ToArray();
        }, treeNode, _permissionService, _stringLocalizerFactory).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(LinkAdminNode treeNode, UpdateEditorContext context)
    {
        var model = new LinkAdminNodeViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix,
            x => x.LinkUrl,
            x => x.LinkText,
            x => x.Target,
            x => x.IconClass,
            x => x.SelectedPermissionNames);

        treeNode.LinkText = model.LinkText;
        treeNode.LinkUrl = model.LinkUrl;
        treeNode.Target = model.Target;
        treeNode.IconClass = model.IconClass;

        var selectedPermissions = model.SelectedPermissionNames == null
            ? []
            : model.SelectedPermissionNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var permissions = await _permissionService.FindByNamesAsync(selectedPermissions);
        treeNode.PermissionNames = permissions.Select(p => p.Name).ToArray();

        return Edit(treeNode, context);
    }
}
