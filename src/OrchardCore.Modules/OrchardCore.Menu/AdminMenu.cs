using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Menu;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "contentTypeId", "Menu" },
        { "Area", "OrchardCore.Contents" },
        { "Options.SelectedContentType", "Menu" },
        { "Options.CanCreateSelectedContentType", true }
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Content"], design => design
                .Add(S["Menus"], S["Menus"].PrefixPosition(), menus => menus
                    .Permission(Permissions.ManageMenu)
                    .Action("List", "Admin", _routeValues)
                    .LocalNav()
                )
            );

        return Task.CompletedTask;
    }
}
