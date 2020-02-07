using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Import/Export"], S["Import/Export"], import => import
                        .Add(S["Deployment Plans"], "5", deployment => deployment
                            .Action("Index", "DeploymentPlan", new { area = "OrchardCore.Deployment" })
                            .Permission(Permissions.Export)
                            .LocalNav()
                        )
                        .Add(S["Package Import"], "5", deployment => deployment
                            .Action("Index", "Import", new { area = "OrchardCore.Deployment" })
                            .Permission(Permissions.Import)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
