using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Google;

public sealed class GoogleAuthenticationAdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleAuthentication },
    };

    internal readonly IStringLocalizer S;

    public GoogleAuthenticationAdminMenu(IStringLocalizer<GoogleAuthenticationAdminMenu> localizer)
    {
        S = localizer;
    }

    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Security"], security => security
                .Add(S["Authentication"], authentication => authentication
                .Add(S["Google"], S["Google"].PrefixPosition(), google => google
                    .AddClass("google")
                    .Id("google")
                    .Action("Index", "Admin", _routeValues)
                    .Permission(Permissions.ManageGoogleAuthentication)
                    .LocalNav()
                )
            )
        );

        return ValueTask.CompletedTask;
    }
}
