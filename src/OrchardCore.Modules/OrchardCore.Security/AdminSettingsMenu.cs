using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Security.Drivers;
using OrchardCore.Settings;

namespace OrchardCore.Security;

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
            .Add(S["Security"], NavigationConstants.AdminMenuSecurityPosition, security => security
                .Add(S["Security Headers"], S["Security Headers"].PrefixPosition(), headers => headers
                    .Action(GetRouteValues(SecuritySettingsDisplayDriver.GroupId))
                    .Permission(SecurityPermissions.ManageSecurityHeadersSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
