using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId;

public sealed class ValidationAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    internal readonly IStringLocalizer S;

    public ValidationAdminMenu(IStringLocalizer<ValidationAdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), openId => openId
                    .Add(S["Token Validation"], S["Token Validation"].PrefixPosition(), validation => validation
                        .Action("Index", "ValidationConfiguration", "OrchardCore.OpenId")
                        .Permission(OpenIdPermissions.ManageValidationSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
