using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Search;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", SearchConstants.SearchSettingsGroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Search"], S["Search"].PrefixPosition(), search => search
                    .Action("Index", "Admin", _routeValues)
                    .AddClass("searchsettings")
                    .Id("searchsettings")
                    .Permission(Permissions.ManageSearchSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
