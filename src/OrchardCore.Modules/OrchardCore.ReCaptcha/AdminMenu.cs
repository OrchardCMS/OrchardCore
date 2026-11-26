using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Navigation;
using OrchardCore.ReCaptcha.Drivers;

namespace OrchardCore.ReCaptcha;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ReCaptchaSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Security"], S["Security"].PrefixPosition(), security => security
                    .Add(S["reCaptcha"], S["reCaptcha"].PrefixPosition(), reCaptcha => reCaptcha
                        .Permission(ReCaptchaPermissions.ManageReCaptchaSettings)
                        .Action("Index", "Admin", s_routeValues)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
