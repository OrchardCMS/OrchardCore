using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Menu;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly Permission s_listMenuContent = ContentTypePermissionsHelper.CreateDynamicPermission(
        ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.ListContent.Name],
        "Menu");

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "contentTypeId", "Menu" },
        { "Area", "OrchardCore.Contents" },
        { "Options.SelectedContentType", "Menu" },
        { "Options.CanCreateSelectedContentType", true },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Content"], design => design
                .Add(S["Menus"], S["Menus"].PrefixPosition(), menus => menus
                    .Permission(s_listMenuContent)
                    .Resource("Menu")
                    .Action("List", "Admin", s_routeValues)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
