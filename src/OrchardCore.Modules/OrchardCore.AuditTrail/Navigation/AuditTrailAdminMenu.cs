using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Controllers;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Navigation;

namespace OrchardCore.AuditTrail.Navigation;

public sealed class AuditTrailAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.AuditTrail" },
        { "correlationId", string.Empty },
    };

    private static readonly RouteValueDictionary _settingsRouteValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", AuditTrailSettingsGroup.Id },
    };

    private readonly IStringLocalizer S;

    public AuditTrailAdminMenu(IStringLocalizer<AuditTrailAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Audit Trail"], NavigationConstants.AdminMenuAuditTrailPosition, configuration => configuration
                    .AddClass("audittrail")
                    .Id("audittrail")
                    .Action(nameof(AdminController.Index), "Admin", _routeValues)
                    .Permission(AuditTrailPermissions.ViewAuditTrail)
                    .LocalNav()
                , priority: 1)
                .Add(S["Configuration"], configuration => configuration
                     .Add(S["Settings"], settings => settings
                        .Add(S["Audit Trail"], S["Audit Trail"].PrefixPosition(), auditTrail => auditTrail
                            .AddClass("audittrail")
                            .Id("audittrailSettings")
                            .Action("Index", "Admin", _routeValues)
                            .Permission(AuditTrailPermissions.ManageAuditTrailSettings)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Audit Trail"], S["Audit Trail"].PrefixPosition(), configuration => configuration
                    .AddClass("audittrail")
                    .Id("audittrail")
                    .Action(nameof(AdminController.Index), "Admin", _routeValues)
                    .Permission(AuditTrailPermissions.ViewAuditTrail)
                    .LocalNav()
                )
            )
            .Add(S["Settings"], settings => settings
                .Add(S["Audit Trail"], S["Audit Trail"].PrefixPosition(), auditTrail => auditTrail
                    .AddClass("audittrail")
                    .Id("audittrailSettings")
                    .Action("Index", "Admin", _settingsRouteValues)
                    .Permission(AuditTrailPermissions.ManageAuditTrailSettings)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
