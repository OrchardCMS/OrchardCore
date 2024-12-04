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
            .Add(S["Access Control"], accessControl => accessControl
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .AddClass("openid")
                    .Id("openid")
                    .Add(S["Applications"], S["Applications"].PrefixPosition(), applications => applications
                        .Action("Index", "Application", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageApplications)
                        .LocalNav()
                    )
                    .Add(S["Scopes"], S["Scopes"].PrefixPosition(), applications => applications
                        .Action("Index", "Scope", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageScopes)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
