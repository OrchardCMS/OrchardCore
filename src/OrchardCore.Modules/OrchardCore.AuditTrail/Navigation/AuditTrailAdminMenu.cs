using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Controllers;
using OrchardCore.Navigation;

namespace OrchardCore.AuditTrail.Navigation
{
    public class AuditTrailAdminMenu : INavigationProvider
    {
        private static readonly RouteValueDictionary _routeValues = new()
        {
            { "area", "OrchardCore.AuditTrail" },
            { "correlationId", string.Empty },
        };

        protected readonly IStringLocalizer S;

        public AuditTrailAdminMenu(IStringLocalizer<AuditTrailAdminMenu> stringLocalizer)
        {
            S = stringLocalizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!NavigationHelper.IsAdminMenu(name))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Audit Trail"], NavigationConstants.AdminMenuAuditTrailPosition, configuration => configuration
                    .AddClass("audittrail")
                    .Id("audittrail")
                    .Action(nameof(AdminController.Index), "Admin", _routeValues)
                    .Permission(AuditTrailPermissions.ViewAuditTrail)
                    .LocalNav()
                );

            return Task.CompletedTask;
        }
    }
}
