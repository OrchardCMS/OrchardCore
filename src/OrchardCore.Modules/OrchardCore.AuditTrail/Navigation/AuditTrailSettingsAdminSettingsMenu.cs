using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.AuditTrail.Navigation;

public sealed class AuditTrailSettingsAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public AuditTrailSettingsAdminSettingsMenu(IStringLocalizer<AuditTrailSettingsAdminSettingsMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
             .Add(S["Site"], site => site
                 .Add(S["Audit Trail"], S["Audit Trail"].PrefixPosition(), auditTrail => auditTrail
                    .AddClass("audittrail")
                    .Id("audittrailSettings")
                    .Action(GetRouteValues(AuditTrailSettingsGroup.Id))
                    .Permission(AuditTrailPermissions.ManageAuditTrailSettings)
                    .LocalNav()
                 )
             );

        return ValueTask.CompletedTask;
    }
}
