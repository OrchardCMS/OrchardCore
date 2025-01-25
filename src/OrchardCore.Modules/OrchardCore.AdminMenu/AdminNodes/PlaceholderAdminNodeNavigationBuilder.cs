using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.AdminNodes;

public class PlaceholderAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissionService;

    public PlaceholderAdminNodeNavigationBuilder(
        IPermissionService permissionService,
        ILogger<PlaceholderAdminNodeNavigationBuilder> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    public string Name => nameof(PlaceholderAdminNode);

    public Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
    {
        var node = menuItem as PlaceholderAdminNode;

        if (node == null || string.IsNullOrEmpty(node.LinkText) || !node.Enabled)
        {
            return Task.CompletedTask;
        }

        return builder.AddAsync(new LocalizedString(node.LinkText, node.LinkText), async itemBuilder =>
        {
            itemBuilder.Priority(node.Priority);
            itemBuilder.Position(node.Position);

            if (node.PermissionNames.Length > 0)
            {
                // Find the actual permissions and apply them to the menu.
                itemBuilder.Permissions(await _permissionService.FindByNamesAsync(node.PermissionNames));
            }

            // Add adminNode's IconClass property values to menuItem.Classes.
            // Add them with a prefix so that later the shape template can extract them to use them on a <i> tag.
            node.IconClass?.Split(' ').ToList().ForEach(c => itemBuilder.AddClass("icon-class-" + c));

            // Let children build themselves inside this MenuItem
            // todo: this logic can be shared by all TreeNodeNavigationBuilders
            foreach (var childTreeNode in menuItem.Items)
            {
                try
                {
                    var treeBuilder = treeNodeBuilders.FirstOrDefault(x => x.Name == childTreeNode.GetType().Name);
                    await treeBuilder.BuildNavigationAsync(childTreeNode, itemBuilder, treeNodeBuilders);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childTreeNode.GetType().Name);
                }
            }
        });
    }
}
