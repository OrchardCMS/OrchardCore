using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ServerAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ServerAdminMenu(IStringLocalizer<ServerAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder

            .Add(S["Tools"], tools => tools
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .Id("openid")
                    .Add(S["Authorization server"], S["Authorization server"].PrefixPosition(), server => server
                        .Action("Index", "ServerConfiguration", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageServerSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
