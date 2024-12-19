using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Controllers;
using OrchardCore.Navigation;

namespace OrchardCore.AuditTrail.Navigation;

public sealed class AuditTrailAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.AuditTrail" },
        { "correlationId", string.Empty },
    };

    internal readonly IStringLocalizer S;

    public AuditTrailAdminMenu(IStringLocalizer<AuditTrailAdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["Audit Trail"], NavigationConstants.AdminMenuAuditTrailPosition, configuration => configuration
                .AddClass("audittrail")
                .Id("audittrail")
                .Action(nameof(AdminController.Index), "Admin", _routeValues)
                .Permission(AuditTrailPermissions.ViewAuditTrail)
                .LocalNav()
            );

        return ValueTask.CompletedTask;
    }
}
