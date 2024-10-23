using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Search;

public sealed class AdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AdminSettingsMenu(IStringLocalizer<AdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Site"], site => site
                .Add(S["Search"], S["Search"].PrefixPosition(), search => search
                    .AddClass("searchsettings")
                    .Id("searchsettings")
                    .Action(GetRouteValues(SearchConstants.SearchSettingsGroupId))
                    .Permission(Permissions.ManageSearchSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
