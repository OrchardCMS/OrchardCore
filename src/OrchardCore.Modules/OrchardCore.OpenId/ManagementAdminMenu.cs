using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ManagementAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ManagementAdminMenu(IStringLocalizer<ManagementAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Security"], security => security
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .AddClass("openid")
                    .Id("openid")
                    .Add(S["Applications"], applications => applications
                        .Action("Index", "Application", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageApplications)
                        .LocalNav()
                    )
                    .Add(S["Scopes"], applications => applications
                        .Action("Index", "Scope", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageScopes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
