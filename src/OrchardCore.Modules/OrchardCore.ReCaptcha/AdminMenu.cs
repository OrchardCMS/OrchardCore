using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.ReCaptcha.Drivers;

namespace OrchardCore.ReCaptcha;

public sealed class AdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ReCaptchaSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Security"], security => security
                .Add(S["Settings"], settings => settings
                    .Add(S["reCaptcha"], S["reCaptcha"].PrefixPosition(), reCaptcha => reCaptcha
                        .Permission(Permissions.ManageReCaptchaSettings)
                        .Action("Index", "Admin", _routeValues)
                        .LocalNav()
                    )
                )
            );

        return Task.CompletedTask;
    }
}
