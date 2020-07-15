using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Controllers;
using OrchardCore.AuditTrail.Permissions;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Navigation
{
    public class AuditTrailAdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer T;


        public AuditTrailAdminMenu(IStringLocalizer<AuditTrailAdminMenu> stringLocalizer)
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
                .Add(T["Audit Trail"], "10", configuration => configuration
                    .AddClass("audittrail").Id("audittrail")
                    .Action(nameof(AdminController.Index), "Admin", new { area = "OrchardCore.AuditTrail" })
                    .Permission(AuditTrailPermissions.ViewAuditTrail)
                    .LocalNav());

            return Task.CompletedTask;
        }
    }
}
