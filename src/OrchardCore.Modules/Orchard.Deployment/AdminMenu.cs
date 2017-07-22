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
                .Add(T["Content"], content => content
                    .Add(T["Import/Export"], "10", import => import
                        .Add(T["Deployment Plans"], "5", deployment => deployment
                            .Action("Index", "DeploymentPlan", new { area = "Orchard.Deployment" })
                            .Permission(Permissions.Export)
                            .LocalNav()
                        )
                    )
                );
        }
    }
}
