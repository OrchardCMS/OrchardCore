using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Drivers;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.Navigation;

namespace OrchardCore.AuditTrail.Navigation
{
    public class AuditTrailSettingsAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer T;

        public AuditTrailSettingsAdminMenu(IStringLocalizer<AuditTrailSettingsAdminMenu> stringLocalizer)
        {
            T = stringLocalizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                 .Add(T["Configuration"], configuration => configuration
                     .Add(T["Settings"], settings => settings
                        .Add(T["Audit Trail"], T["Audit Trail"], settings => settings
                        .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AuditTrailSettingsDisplayDriver.AuditTrailSettingsGroupId })
                        .Permission(AuditTrailPermissions.ManageAuditTrailSettings)
                        .LocalNav())));

            return Task.CompletedTask;
        }
    }
}
