using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Microsoft.Authentication;

public sealed class AdminMenuAAD : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", MicrosoftAuthenticationConstants.Features.AAD },
    };

    private readonly IStringLocalizer S;

    public AdminMenuAAD(IStringLocalizer<AdminMenuAAD> stringLocalizer)
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
                        .Add(S["Microsoft Entra ID"], S["Microsoft Entra ID"].PrefixPosition(), entraId => entraId
                            .AddClass("microsoft-entra-id")
                            .Id("microsoftentraid")
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
                        .Add(S["Microsoft Entra ID"], S["Microsoft Entra ID"].PrefixPosition(), entraId => entraId
                            .AddClass("microsoft-entra-id")
                            .Id("microsoftentraid")
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
