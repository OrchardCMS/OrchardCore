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
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
               .Add(S["Security"], security => security
                   .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                       .AddClass("openid")
                       .Id("openid")
                       .Add(S["Settings"], S["Settings"].PrefixPosition(), settings => settings
                            .Add(S["Token Validation"], S["Token Validation"].PrefixPosition(), validation => validation
                                .Action("Index", "ValidationConfiguration", "OrchardCore.OpenId")
                                .Permission(Permissions.ManageValidationSettings)
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
                        .Add(S["Token Validation"], S["Token Validation"].PrefixPosition(), validation => validation
                            .Action("Index", "ValidationConfiguration", "OrchardCore.OpenId")
                            .Permission(Permissions.ManageValidationSettings)
                            .LocalNav()
                        )
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
