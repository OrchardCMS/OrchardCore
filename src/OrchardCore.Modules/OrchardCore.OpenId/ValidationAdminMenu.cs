using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ValidationAdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ValidationAdminMenu(IStringLocalizer<ValidationAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Tools"], tools => tools
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .Id("openid")
                        .Add(S["Token validation"], S["Token validation"].PrefixPosition(), validation => validation
                        .Action("Index", "ValidationConfiguration", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageValidationSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
