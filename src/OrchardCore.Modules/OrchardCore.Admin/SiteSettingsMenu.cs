using Microsoft.Extensions.Localization;
using OrchardCore.Admin.Drivers;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Admin;

public sealed class SiteSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public SiteSettingsMenu(IStringLocalizer<SiteSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Site"], "before", site => site
                .Id("siteSettings")
                .Add(S["Admin"], S["Admin"].PrefixPosition(), admin => admin
                    .AddClass("admin")
                    .Id("admin")
                    .Action(GetRouteValues(AdminSiteSettingsDisplayDriver.GroupId))
                    .Permission(PermissionsAdminSettings.ManageAdminSettings)
                    .LocalNav()
                ),
                priority: 1
            );

        return ValueTask.CompletedTask;
    }
}
