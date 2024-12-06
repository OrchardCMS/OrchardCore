using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Deployment;

public sealed class AdminMenu : AdminNavigationProvider
{
    internal readonly IStringLocalizer S;

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
                        .Add(S["Deployment plans"], S["Deployment plans"].PrefixPosition(), deployment => deployment
                            .Action("Index", "DeploymentPlan", "OrchardCore.Deployment")
                            .Permission(CommonPermissions.Export)
                            .LocalNav()
                        )
                        .Add(S["Package import"], S["Package import"].PrefixPosition(), deployment => deployment
                            .Action("Index", "Import", "OrchardCore.Deployment")
                            .Permission(CommonPermissions.Import)
                            .LocalNav()
                        )
                        .Add(S["JSON import"], S["JSON import"].PrefixPosition(), deployment => deployment
                            .Action("Json", "Import", "OrchardCore.Deployment")
                            .Permission(CommonPermissions.Import)
                            .LocalNav()
                        )
                    )
                );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Tools"], tools => tools
                .Add(S["Deployments"], S["Deployments"].PrefixPosition(), import => import
                    .Add(S["Plans"], "1", deployment => deployment
                        .Action("Index", "DeploymentPlan", "OrchardCore.Deployment")
                        .Permission(CommonPermissions.Export)
                        .LocalNav()
                    )
                    .Add(S["Package import"], S["Package import"].PrefixPosition(), deployment => deployment
                        .Action("Index", "Import", "OrchardCore.Deployment")
                        .Permission(CommonPermissions.Import)
                        .LocalNav()
                    )
                    .Add(S["JSON import"], S["JSON import"].PrefixPosition(), deployment => deployment
                        .Action("Json", "Import", "OrchardCore.Deployment")
                        .Permission(CommonPermissions.Import)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
