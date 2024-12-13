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
        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Import/Export"], S["Import/Export"].PrefixPosition(), import => import
                    .Add(S["Deployment Plans"], S["Deployment Plans"].PrefixPosition(), deployment => deployment
                        .Action("Index", "DeploymentPlan", "OrchardCore.Deployment")
                        .Permission(CommonPermissions.Export)
                        .LocalNav()
                    )
                    .Add(S["Package Import"], S["Package Import"].PrefixPosition(), deployment => deployment
                        .Action("Index", "Import", "OrchardCore.Deployment")
                        .Permission(CommonPermissions.Import)
                        .LocalNav()
                    )
                    .Add(S["JSON Import"], S["JSON Import"].PrefixPosition(), deployment => deployment
                        .Action("Json", "Import", "OrchardCore.Deployment")
                        .Permission(CommonPermissions.Import)
                        .LocalNav()
                    )
                )
            );

        return ValueTask.CompletedTask;
    }
}
