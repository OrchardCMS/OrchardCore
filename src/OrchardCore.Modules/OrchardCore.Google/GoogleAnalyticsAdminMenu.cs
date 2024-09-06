using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Google;

public sealed class GoogleAnalyticsAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleAnalytics },
    };

    internal readonly IStringLocalizer S;

    public GoogleAnalyticsAdminMenu(IStringLocalizer<GoogleAnalyticsAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Google Analytics"], S["Google Analytics"].PrefixPosition(), google => google
                        .AddClass("googleAnalytics").Id("googleAnalytics")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageGoogleAnalytics)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
