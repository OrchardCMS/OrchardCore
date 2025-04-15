using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment;

public sealed class AdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        if (NavigationHelper.UseLegacyFormat())
        {
            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Import/Export"], S["Import/Export"].PrefixPosition(), import => import
                        .Add(S["Deployment Plans"], S["Deployment Plans"].PrefixPosition(), deployment => deployment
                            .Action("Index", "DeploymentPlan", "OrchardCore.Deployment")
                            .Permission(DeploymentPermissions.Export)
                            .LocalNav()
                        )
                        .Add(S["Package Import"], S["Package Import"].PrefixPosition(), deployment => deployment
                            .Action("Index", "Import", "OrchardCore.Deployment")
                            .Permission(DeploymentPermissions.Import)
                            .LocalNav()
                        )
                        .Add(S["JSON Import"], S["JSON Import"].PrefixPosition(), deployment => deployment
                            .Action("Json", "Import", "OrchardCore.Deployment")
                            .Permission(DeploymentPermissions.Import)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Deployments"], S["Deployments"].PrefixPosition(), import => import
                    .Add(S["Plans"], S["Plans"].PrefixPosition("1"), deployment => deployment
                        .Action("Index", "DeploymentPlan", "OrchardCore.Deployment")
                        .Permission(DeploymentPermissions.Export)
                        .LocalNav()
                    )
                    .Add(S["Package Import"], S["Package Import"].PrefixPosition(), deployment => deployment
                        .Action("Index", "Import", "OrchardCore.Deployment")
                        .Permission(DeploymentPermissions.Import)
                        .LocalNav()
                    )
                    .Add(S["JSON Import"], S["JSON Import"].PrefixPosition(), deployment => deployment
                        .Action("Json", "Import", "OrchardCore.Deployment")
                        .Permission(DeploymentPermissions.Import)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
