using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Navigation;
using YesSql;

namespace OrchardCore.AdminMenu.Services
{
    // Retrieves all instances of "IAdminNodeNavigationBuilder"
    // Those are classes that add new "AdminNodes" to a "NavigationBuilder" using custom logic specific to the module that register them.
    // This class handles their inclusion on the admin menu.
    // This class is itself one more INavigationProvider so it can be called from this module's AdminMenu.cs
    public class AdminMenuNavigationProvidersCoordinator : INavigationProvider
    {
        private readonly IAdminMenuService _adminMenuService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEnumerable<IAdminNodeNavigationBuilder> _nodeBuilders;
        private readonly ILogger Logger;

        public AdminMenuNavigationProvidersCoordinator(
            IAdminMenuService adminMenuService,
            IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor,
            IEnumerable<IAdminNodeNavigationBuilder> nodeBuilders,
            ILogger<AdminMenuNavigationProvidersCoordinator> logger)
        {
            _adminMenuService = adminMenuService;
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            _nodeBuilders = nodeBuilders;
            Logger = logger;
        }

        // We only add them if the caller uses the string "adminMenu").
        // todo: use a public constant for the string
        public async Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "adminMenu", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var trees = (await _adminMenuService.GetAsync())
                .Where(m => m.Enabled && m.MenuItems.Count > 0);

            foreach (var tree in trees)
            {
                if (await _authorizationService.AuthorizeAsync(
                    _httpContextAccessor.HttpContext?.User,
                    Permissions.CreatePermissionForAdminMenu(tree.Name)))
                {
                    await BuildTreeAsync(tree, builder);
                }
            }
        }

        private async Task BuildTreeAsync(Models.AdminMenu tree, NavigationBuilder builder)
        {
            foreach (MenuItem node in tree.MenuItems)
            {
                var nodeBuilder = _nodeBuilders.Where(x => x.Name == node.GetType().Name).FirstOrDefault();
                
                if (nodeBuilder != null)
                {
                    await nodeBuilder.BuildNavigationAsync(node, builder, _nodeBuilders);
                }
                else
                {
                    Logger.LogError("No Builder registered for admin node of type '{TreeNodeName}'", node.GetType().Name);
                }
            }
        }
    }
}
