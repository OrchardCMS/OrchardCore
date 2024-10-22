using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.OpenId;

public sealed class ClientSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ClientSettingsMenu(IStringLocalizer<ClientSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], openId => openId
                .Add(S["OpenID Connect client"], S["OpenID Connect client"].PrefixPosition(), client => client
                    .Action(GetRouteValues("OrchardCore.OpenId.Client"))
                    .Permission(Permissions.ManageClientSettings)
                    .LocalNav()
                )
             );

        return ValueTask.CompletedTask;
    }
}
