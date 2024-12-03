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
            .Add(S["Settings"], settings => settings
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .Add(S["Token validation"], validation => validation
                        .Action("Index", "ValidationConfiguration", "OrchardCore.OpenId")
                        .Permission(Permissions.ManageValidationSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
