using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Layers.Drivers;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Layers;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LayerSiteSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Design"], design => design
                .Add(S["Widgets"], S["Widgets"].PrefixPosition(), widgets => widgets
                    .Permission(Permissions.ManageLayers)
                    .Action("Index", "Admin", "OrchardCore.Layers")
                    .LocalNav()
                )
            )
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Zones"], S["Zones"].PrefixPosition(), zones => zones
                    .Action("Index", "Admin", s_routeValues)
                    .Permission(Permissions.ManageLayers)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
