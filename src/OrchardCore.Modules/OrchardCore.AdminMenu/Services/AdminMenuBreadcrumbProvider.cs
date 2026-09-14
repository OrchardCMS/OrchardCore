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

    internal readonly IStringLocalizer S;

    public AdminMenuBreadcrumbProvider(IStringLocalizer<AdminMenuBreadcrumbProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder)
    {
        switch (builder.Name)
        {
            case AdminMenuBreadcrumbs.List:
                AddList(builder);
                break;

            case AdminMenuBreadcrumbs.Create:
                AddList(builder);
                builder.Add(S["Create Admin Menu"], item => item.Id("Menu"));
                break;

            case AdminMenuBreadcrumbs.Edit:
                AddList(builder);
                builder.Add(S["Edit Admin Menu: {0}", builder.GetData<string>(AdminMenuBreadcrumbs.MenuNameKey)],
                    item => item.Id("Menu"));
                break;

            case AdminMenuBreadcrumbs.Nodes:
                AddList(builder);
                builder.Add(S["Edit Nodes for '{0}'", builder.GetData<string>(AdminMenuBreadcrumbs.MenuNameKey)],
                    item => item.Id("Nodes"));
                break;

            case AdminMenuBreadcrumbs.NodeCreate:
                AddList(builder);
                builder.Add(S["Create Node"], item => item.Id("Node"));
                break;

            case AdminMenuBreadcrumbs.NodeEdit:
                AddList(builder);
                builder.Add(S["Edit Node"], item => item.Id("Node"));
                break;
        }

        return ValueTask.CompletedTask;
    }

    private void AddList(BreadcrumbBuilder builder)
        => builder.Add(S["Admin Menus"], item => item
            .Id("AdminMenus")
            .Action("List", "Menu", s_routeValues)
            .Permission(AdminMenuPermissions.ManageAdminMenu));
}
