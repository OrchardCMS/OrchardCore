using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Settings;
using OrchardCore.Navigation;

namespace OrchardCore.AuditTrail.Navigation
{
    public class AuditTrailSettingsAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public AuditTrailSettingsAdminMenu(IStringLocalizer<AuditTrailSettingsAdminMenu> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                 .Add(S["Configuration"], configuration => configuration
                     .Add(S["Settings"], settings => settings
                        .Add(S["Audit Trail"], S["Audit Trail"].PrefixPosition(), settings => settings
                            .AddClass("audittrail").Id("audittrailSettings")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AuditTrailSettingsGroup.Id })
                            .Permission(AuditTrailPermissions.ManageAuditTrailSettings)
                            .LocalNav())));

            return Task.CompletedTask;
        }
    }
}
