using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Google;

public sealed class GoogleAnalyticsAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleAnalytics },
    };

    internal readonly IStringLocalizer S;

    public GoogleAnalyticsAdminMenu(IStringLocalizer<GoogleAnalyticsAdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Integrations"], S["Integrations"].PrefixPosition(), integrations => integrations
                    .Add(S["Google Analytics"], S["Google Analytics"].PrefixPosition(), google => google
                        .AddClass("googleAnalytics").Id("googleAnalytics")
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(Permissions.ManageGoogleAnalytics)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
