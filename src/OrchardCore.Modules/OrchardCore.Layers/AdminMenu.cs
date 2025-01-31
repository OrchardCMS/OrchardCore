using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Layers.Drivers;
using OrchardCore.Navigation;

namespace OrchardCore.Layers;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LayerSiteSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Design"], design => design
                    .Add(S["Settings"], settings => settings
                        .Add(S["Zones"], S["Zones"].PrefixPosition(), zones => zones
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageLayers)
                            .LocalNav()
                        )
                    )
                    .Add(S["Widgets"], S["Widgets"].PrefixPosition(), widgets => widgets
                        .Permission(Permissions.ManageLayers)
                        .Action("Index", "Admin", "OrchardCore.Layers")
                        .LocalNav()
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Design"], design => design
                .Add(S["Widgets"], S["Widgets"].PrefixPosition(), widgets => widgets
                    .Permission(Permissions.ManageLayers)
                    .Action("Index", "Admin", "OrchardCore.Layers")
                    .LocalNav()
                )
            )
            .Add(S["Settings"], settings => settings
                .Add(S["Zones"], S["Zones"].PrefixPosition(), zones => zones
                    .Action("Index", "Admin", _routeValues)
                    .Permission(Permissions.ManageLayers)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
