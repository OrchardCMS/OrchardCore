using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.ReverseProxy.Drivers;
using OrchardCore.Settings;

namespace OrchardCore.ReverseProxy;

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
            .Add(S["General"], general => general
                .Add(S["Reverse Proxy"], S["Reverse Proxy"].PrefixPosition(), entry => entry
                    .AddClass("reverseproxy")
                    .Id("reverseproxy")
                    .Action(GetRouteValues(ReverseProxySettingsDisplayDriver.GroupId))
                    .Permission(Permissions.ManageReverseProxySettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
