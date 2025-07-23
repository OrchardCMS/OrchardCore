using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ServerAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ServerAdminMenu(IStringLocalizer<ClientAdminMenu> stringLocalizer)
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
                            .Add(S["Authorization Server"], S["Authorization Server"].PrefixPosition(), server => server
                                .Action("Index", "ServerConfiguration", "OrchardCore.OpenId")
                                .Permission(OpenIdPermissions.ManageServerSettings)
                                .LocalNav()
                            )
                       )
                   )
               );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .AddClass("openid")
                    .Id("openid")
                    .Add(S["Authorization Server"], S["Authorization Server"].PrefixPosition(), server => server
                        .Action("Index", "ServerConfiguration", "OrchardCore.OpenId")
                        .Permission(OpenIdPermissions.ManageServerSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
