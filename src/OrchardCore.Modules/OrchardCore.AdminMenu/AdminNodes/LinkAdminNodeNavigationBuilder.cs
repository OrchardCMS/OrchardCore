using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.AdminNodes;

public class LinkAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
{
    private readonly ILogger _logger;
    private readonly IPermissionService _permissionService;
    private readonly AdminOptions _adminOptions;

    public LinkAdminNodeNavigationBuilder(
        IPermissionService permissionService,
        IOptions<AdminOptions> adminOptions,
        ILogger<LinkAdminNodeNavigationBuilder> logger)
    {
        _permissionService = permissionService;
        _adminOptions = adminOptions.Value;
        _logger = logger;
    }

    public string Name => nameof(LinkAdminNode);

    public Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
    {
        var node = menuItem as LinkAdminNode;
        if (node == null || string.IsNullOrEmpty(node.LinkText) || !node.Enabled)
        {
            return Task.CompletedTask;
        }

        return builder.AddAsync(new LocalizedString(node.LinkText, node.LinkText), async itemBuilder =>
        {
            var nodeLinkUrl = node.LinkUrl;
            if (!string.IsNullOrEmpty(nodeLinkUrl) && nodeLinkUrl[0] != '/' && !nodeLinkUrl.Contains("://"))
            {
                if (nodeLinkUrl.StartsWith("~/", StringComparison.Ordinal))
                {
                    nodeLinkUrl = nodeLinkUrl[2..];
                }

                // Check if the first segment of 'nodeLinkUrl' is not equal to the admin prefix.
                if (!nodeLinkUrl.StartsWith($"{_adminOptions.AdminUrlPrefix}", StringComparison.OrdinalIgnoreCase) ||
                    (nodeLinkUrl.Length != _adminOptions.AdminUrlPrefix.Length
                    && nodeLinkUrl[_adminOptions.AdminUrlPrefix.Length] != '/'))
                {
                    nodeLinkUrl = $"{_adminOptions.AdminUrlPrefix}/{nodeLinkUrl}";
                }
            }

            // Add the actual link.
            itemBuilder.Url(nodeLinkUrl);
            itemBuilder.Target(node.Target);
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

            // Let children build themselves inside this MenuItem.
            // Todo: This logic can be shared by all TreeNodeNavigationBuilders.
            foreach (var childTreeNode in menuItem.Items)
            {
                try
                {
                    var treeBuilder = treeNodeBuilders.FirstOrDefault(x => x.Name == childTreeNode.GetType().Name);
                    await treeBuilder.BuildNavigationAsync(childTreeNode, itemBuilder, treeNodeBuilders).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childTreeNode.GetType().Name);
                }
            }
        });
    }
}
