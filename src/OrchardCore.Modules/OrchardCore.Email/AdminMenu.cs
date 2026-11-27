using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Email.Controllers;
using OrchardCore.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.Email;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", EmailSettings.GroupId },
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
            .Add(S["Tools"], tools => tools
                .Add(S["Testing"], S["Testing"].PrefixPosition(), testing => testing
                    .Add(S["Email Test"], S["Email Test"].PrefixPosition(), entry => entry
                        .AddClass("emailtest")
                        .Id("emailtest")
                        .Action(nameof(AdminController.Test), typeof(AdminController).ControllerName(), "OrchardCore.Email")
                        .Permission(EmailPermissions.ManageEmailSettings)
                        .LocalNav()
                    )
                )
            )
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Communication"], S["Communication"].PrefixPosition(), communication => communication
                    .Add(S["Email"], S["Email"].PrefixPosition(), entry => entry
                        .AddClass("email")
                        .Id("email")
                        .Action("Index", "Admin", s_routeValues)
                        .Permission(EmailPermissions.ManageEmailSettings)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
