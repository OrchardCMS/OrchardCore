using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Google;

public sealed class GoogleTagManagerAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleTagManager },
    };

    internal readonly IStringLocalizer S;

    public GoogleTagManagerAdminMenu(IStringLocalizer<GoogleTagManagerAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Settings"], settings => settings
                    .Add(S["Google Tag Manager"], S["Google Tag Manager"].PrefixPosition(), google => google
                        .AddClass("googleTagManager")
                        .Id("googleTagManager")
                        .Action("Index", "Admin", _routeValues)
                        .Permission(Permissions.ManageGoogleTagManager)
                        .LocalNav()
                    )
                )
            );


        return ValueTask.CompletedTask;
    }
}
