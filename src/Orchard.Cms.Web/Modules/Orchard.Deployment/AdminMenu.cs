using Microsoft.Extensions.Localization;
using Orchard.Environment.Navigation;
using System;

namespace Orchard.Deployment
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(T["Content"], "10", themes => themes
                    .Add(T["Import"], "0", import => import
                        .Action("Index", "Import", new { area = "Orchard.Deployment" })
                        .Permission(Permissions.Import)
                        .LocalNav()
                    )
                    .Add(T["Deployment"], "1", installed => installed
                        .Action("Index", "DeploymentPlan", new { area = "Orchard.Deployment" })
                        .Permission(Permissions.Export)
                        .LocalNav()
                    )
                );
        }
    }
}
