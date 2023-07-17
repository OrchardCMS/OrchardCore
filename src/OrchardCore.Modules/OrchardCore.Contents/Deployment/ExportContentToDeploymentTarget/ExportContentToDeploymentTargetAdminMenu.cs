using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget
{
    public class ExportContentToDeploymentTargetAdminMenu : INavigationProvider
    {
        protected readonly IStringLocalizer S;

        public ExportContentToDeploymentTargetAdminMenu(IStringLocalizer<AdminMenu> localizer)
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
                        .Add(S["Settings"], settings => settings
                            .Add(S["Export Target Settings"], S["Export Target Settings"].PrefixPosition(), deployment => deployment
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "ExportContentToDeploymentTarget" })
                                .Permission(OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan)
                                .LocalNav()
                            )
                        )
                    )
                );

            return Task.CompletedTask;
        }
    }
}
