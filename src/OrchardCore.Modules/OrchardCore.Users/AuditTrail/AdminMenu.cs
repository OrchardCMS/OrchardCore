using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Users.AuditTrail.Controllers;
using OrchardCore.Users.Drivers;

namespace OrchardCore.Users.AuditTrail;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LoginSettingsDisplayDriver.GroupId },
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
                    .Add(S["User Audit Trail"], S["User Audit Trail"].PrefixPosition(), login => login
                        .Permission(Permissions.ManageUserAuditTrailSettings)
                        .Action(
                            nameof(AuditTrailAdminController.Index),
                            typeof(AuditTrailAdminController).ControllerName(),
                            new { Area = "OrchardCore.Users" })
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
