using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Google;

public sealed class GoogleAuthenticationAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", GoogleConstants.Features.GoogleAuthentication },
    };

    internal readonly IStringLocalizer S;

    public GoogleAuthenticationAdminMenu(IStringLocalizer<GoogleAuthenticationAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
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

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["Authentication"], S["Authentication"].PrefixPosition(), authentication => authentication
                        .Add(S["Google"], S["Google"].PrefixPosition(), google => google
                            .AddClass("google")
                            .Id("google")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageGoogleAuthentication)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
