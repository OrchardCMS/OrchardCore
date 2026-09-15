using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.AdminMenu.Services;

/// <summary>
/// Describes the breadcrumb trails of the admin menus screens.
/// </summary>
public sealed class AdminMenuBreadcrumbProvider : IBreadcrumbProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.AdminMenu" },
    };

    private readonly IAdminMenuService _adminMenuService;

    internal readonly IStringLocalizer S;

    public AdminMenuBreadcrumbProvider(IAdminMenuService adminMenuService, IStringLocalizer<AdminMenuBreadcrumbProvider> stringLocalizer)
    {
        _adminMenuService = adminMenuService;
        S = stringLocalizer;
    }

    public async ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case AdminMenuConstants.List:
                AddList(builder);
                break;

            case AdminMenuConstants.Create:
                AddList(builder);
                builder.Add(S["Create Admin Menu"], item => item.Id("Menu"));
                break;

            case AdminMenuConstants.Edit:
                AddList(builder);
                builder.Add(S["Edit Admin Menu: {0}", builder.GetData<string>(AdminMenuConstants.MenuNameKey)],
                    item => item.Id("Menu"));
                break;

            case AdminMenuConstants.Nodes:
                AddList(builder);
                builder.Add(S["Edit Nodes for '{0}'", builder.GetData<string>(AdminMenuConstants.MenuNameKey)],
                    item => item.Id("Nodes"));
                break;

            case AdminMenuConstants.NodeCreate:
                AddList(builder);
                await AddNodesAsync(builder);
                builder.Add(S["Create Node"], item => item.Id("Node"));
                break;

            case AdminMenuConstants.NodeEdit:
                AddList(builder);
                await AddNodesAsync(builder);
                builder.Add(S["Edit Node"], item => item.Id("Node"));
                break;
        }
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Admin Menus"], item => item
            .Id("AdminMenus")
            .Action("List", "Menu", s_routeValues)
            .Permission(AdminMenuPermissions.ManageAdminMenu));

    // A node is only reached from its admin menu, so its trail leads back through the menu's nodes.
    private async ValueTask AddNodesAsync(BreadcrumbBuilder builder)
    {
        var menuId = builder.GetData<string>(AdminMenuConstants.MenuIdKey);

        if (string.IsNullOrEmpty(menuId))
        {
            return;
        }

        var adminMenuList = await _adminMenuService.GetAdminMenuListAsync();
        var adminMenu = _adminMenuService.GetAdminMenuById(adminMenuList, menuId);

        builder.Add(S["Edit Nodes for '{0}'", adminMenu?.Name], item => item
            .Id("Nodes")
            .Action("List", "Node", new RouteValueDictionary(s_routeValues)
            {
                { "id", menuId },
            })
            .Permission(AdminMenuPermissions.ManageAdminMenu));
    }
}
