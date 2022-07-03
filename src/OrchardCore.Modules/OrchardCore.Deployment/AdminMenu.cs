using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

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
                    .Add(S["Import/Export"], S["Import/Export"].PrefixPosition(), import => import
                        .Add(S["Deployment plans"], S["Deployment plans"].PrefixPosition(), deployment => deployment
                            .Action("Index", "DeploymentPlan", new { area = "OrchardCore.Deployment" })
                            .Permission(Permissions.Export)
                            .LocalNav()
                        )
                        .Add(S["Package import"], S["Package import"].PrefixPosition(), deployment => deployment
                            .Action("Index", "Import", new { area = "OrchardCore.Deployment" })
                            .Permission(Permissions.Import)
                            .LocalNav()
                        )
                        .Add(S["JSON import"], S["JSON import"].PrefixPosition(), deployment => deployment
                            .Action("Json", "Import", new { area = "OrchardCore.Deployment" })
                            .Permission(Permissions.Import)
                            .LocalNav()
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
