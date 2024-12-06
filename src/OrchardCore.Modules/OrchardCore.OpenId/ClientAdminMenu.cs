using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ClientAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _clientRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", "OrchardCore.OpenId.Client" },
    };


    internal readonly IStringLocalizer S;

    public ClientAdminMenu(IStringLocalizer<ClientAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
               .Add(S["Security"], security => security
                   .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                       .AddClass("openid")
                       .Id("openid")
                       .Add(S["Settings"], S["Settings"].PrefixPosition(), settings => settings
                           .Add(S["Authentication client"], S["Authentication client"].PrefixPosition(), client => client
                               .Action("Index", "Admin", _clientRouteValues)
                               .Permission(Permissions.ManageClientSettings)
                               .LocalNav()
                           )
                       )
                   )
               );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                        .Add(S["Authentication client"], S["Authentication client"].PrefixPosition(), client => client
                            .Action("Index", "Admin", _clientRouteValues)
                            .Permission(Permissions.ManageClientSettings)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
