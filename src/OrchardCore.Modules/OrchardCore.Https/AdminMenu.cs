using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Https.Drivers;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Https;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", HttpsSettingsDisplayDriver.GroupId },
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
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["HTTPS"], S["HTTPS"].PrefixPosition(), https => https
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(Permissions.ManageHttps)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
