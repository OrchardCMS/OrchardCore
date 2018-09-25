using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Deployment
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            T = localizer;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder
                .Add(T["Content"], content => content
                    .Add(T["Import/Export"], "10", import => import
                        .Add(T["Deployment Plans"], "5", deployment => deployment
                            .Action("Index", "DeploymentPlan", new { area = "OrchardCore.Deployment" })
                            .Permission(Permissions.Export)
                            .LocalNav()
                        )
                        .Add(T["Package Import"], "5", deployment => deployment
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
