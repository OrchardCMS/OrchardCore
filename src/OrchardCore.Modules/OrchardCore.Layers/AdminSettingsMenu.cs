using Microsoft.Extensions.Localization;
using OrchardCore.Layers.Drivers;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Layers;

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
                .Add(S["Zones"], S["Zones"].PrefixPosition(), zones => zones
                    .Action(GetRouteValues(LayerSiteSettingsDisplayDriver.GroupId))
                    .Permission(Permissions.ManageLayers)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
