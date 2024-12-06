using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Microsoft.Authentication;

public sealed class AdminMenuMicrosoftAccount : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", MicrosoftAuthenticationConstants.Features.MicrosoftAccount },
    };

    internal readonly IStringLocalizer S;

    public AdminMenuMicrosoftAccount(IStringLocalizer<AdminMenuMicrosoftAccount> stringLocalizer)
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
                        .Add(S["Microsoft"], S["Microsoft"].PrefixPosition(), microsoft => microsoft
                            .AddClass("microsoft")
                            .Id("microsoft")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageMicrosoftAuthentication)
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
                        .Add(S["Microsoft"], S["Microsoft"].PrefixPosition(), microsoft => microsoft
                            .AddClass("microsoft")
                            .Id("microsoft")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(Permissions.ManageMicrosoftAuthentication)
                            .LocalNav()
                        )
                    )
                )
           );

        return ValueTask.CompletedTask;
    }
}
