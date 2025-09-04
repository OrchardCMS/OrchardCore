using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Services;

// Retrieves all instances of 'IAdminNodeNavigationBuilder'.
// Those are classes that add new 'AdminNodes' to a 'NavigationBuilder' using custom logic specific to the module that register them.
// This class handles their inclusion on the admin menu.
// This class is itself one more 'INavigationProvider' so it can be called from this module's AdminMenu.cs.
public sealed class AdminMenuNavigationProvidersCoordinator : INavigationProvider
{
    private readonly IAdminMenuService _adminMenuService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEnumerable<IAdminNodeNavigationBuilder> _nodeBuilders;
    private readonly ILogger _logger;

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
        _logger = logger;
    }

    public async ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        // We only add them if the caller uses the string "adminMenu".
        if (name != NavigationConstants.AdminMenuId)
        {
            return;
        }

        var trees = (await _adminMenuService.GetAdminMenuListAsync())
            .AdminMenu.Where(m => m.Enabled && m.MenuItems.Count > 0);

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
        foreach (var node in tree.MenuItems)
        {
            var nodeBuilder = _nodeBuilders.FirstOrDefault(x => x.Name == node.GetType().Name);

            if (nodeBuilder != null)
            {
                await nodeBuilder.BuildNavigationAsync(node, builder, _nodeBuilders);
            }
            else
            {
                _logger.LogError("No Builder registered for admin node of type '{TreeNodeName}'", node.GetType().Name);
            }
        }
    }
}
