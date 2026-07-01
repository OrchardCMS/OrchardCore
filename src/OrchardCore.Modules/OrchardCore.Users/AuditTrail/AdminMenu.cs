using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Users.AuditTrail.Controllers;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.AuditTrail;

public sealed class AdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", LoginSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Settings"], settings => settings
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
