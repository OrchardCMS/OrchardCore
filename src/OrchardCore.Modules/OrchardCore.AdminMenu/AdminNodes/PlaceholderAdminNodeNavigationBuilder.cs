using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminMenu.Services;
using OrchardCore.Navigation;
using OrchardCore.Security.Services;

namespace OrchardCore.AdminMenu.AdminNodes
{
    public class PlaceholderAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
    {
        private readonly ILogger _logger;
        private readonly IPermissionsService _permissionService;

        public PlaceholderAdminNodeNavigationBuilder(ILogger<PlaceholderAdminNodeNavigationBuilder> logger, IPermissionsService permissionService)
        {
            _logger = logger;
            _permissionService = permissionService;
        }

        public string Name => typeof(PlaceholderAdminNode).Name;

        public Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            var node = menuItem as PlaceholderAdminNode;

            if (node == null || String.IsNullOrEmpty(node.LinkText) || !node.Enabled)
            {
                return Task.CompletedTask;
            }

            return builder.AddAsync(new LocalizedString(node.LinkText, node.LinkText), async itemBuilder =>
            {
                itemBuilder.Priority(node.Priority);
                itemBuilder.Position(node.Position);

                var allInstalledPermissions = await _permissionService.GetInstalledPermissionsAsync();
                //add to permissions only the ones that are existing
                foreach (var savedPermission in node.Permissions)
                {
                    var realPermission = allInstalledPermissions.Where(p => p.Name == savedPermission.Name).FirstOrDefault();
                    if (realPermission != null)
                    {
                        itemBuilder.Permission(realPermission);
                    }
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
                        var treeBuilder = treeNodeBuilders.Where(x => x.Name == childTreeNode.GetType().Name).FirstOrDefault();
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
}
