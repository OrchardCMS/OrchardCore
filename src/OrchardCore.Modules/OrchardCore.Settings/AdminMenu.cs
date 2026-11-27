using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings.Drivers;

namespace OrchardCore.Settings;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_generalRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", DefaultSiteSettingsDisplayDriver.GroupId },
    };

    private static readonly RouteValueDictionary s_debuggingRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", DebugSettingsDisplayDriver.GroupId },
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
            .Add(S["Tools"], "end.50", tools => tools
                .Id("tools")
                .AddClass("tools")
            , priority: 1)
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), "end.100", settings => settings
                .Id("settings")
                .AddClass("settings")
                .Add(_localizerFactory.Localize(CommonLocalizationSources.General), "before", general => general
                    .AddClass("general")
                    .Id("general")
                    .Action("Index", "Admin", s_generalRouteValues)
                    .Permission(SettingsPermissions.ManageGeneralSettings)
                    .LocalNav()
                )
                .Add(S["Debugging"], "after", debugging => debugging
                    .AddClass("debugging")
                    .Id("debugging")
                    .Action("Index", "Admin", s_debuggingRouteValues)
                    .Permission(SettingsPermissions.ManageDebuggingSettings)
                    .LocalNav()
                )
            , priority: 1);

        return ValueTask.CompletedTask;
    }
}
