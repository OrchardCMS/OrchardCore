using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook;

public sealed class AdminMenuLogin : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", FacebookConstants.Features.Login },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuLogin(IStringLocalizer<AdminMenuLogin> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Security"], security => security
                    .Add(S["Authentication"], S["Authentication"].PrefixPosition(), authentication => authentication
                        .Add(S["Meta"], S["Meta"].PrefixPosition(), meta => meta
                            .AddClass("facebook")
                            .Id("facebook")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageFacebookApp)
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
                        .Add(S["Meta"], S["Meta"].PrefixPosition(), meta => meta
                            .AddClass("facebook")
                            .Id("facebook")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageFacebookApp)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
