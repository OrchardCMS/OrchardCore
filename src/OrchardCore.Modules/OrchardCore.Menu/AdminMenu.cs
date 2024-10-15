using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Menu;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "contentTypeId", "Menu" },
        { "Area", "OrchardCore.Contents" },
        { "Options.SelectedContentType", "Menu" },
        { "Options.CanCreateSelectedContentType", true }
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
                    .Permission(Permissions.ManageMenu)
                    .Action("List", "Admin", _routeValues)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
