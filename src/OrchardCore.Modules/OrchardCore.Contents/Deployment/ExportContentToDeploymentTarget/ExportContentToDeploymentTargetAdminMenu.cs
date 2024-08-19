using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetAdminMenu : INavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ExportContentToDeploymentTargetSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public ExportContentToDeploymentTargetAdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!NavigationHelper.IsAdminMenu(name))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Configuration"], configuration => configuration
                .Add(S["Import/Export"], S["Import/Export"].PrefixPosition(), import => import
                    .Add(S["Settings"], settings => settings
                        .Add(S["Export Target Settings"], S["Export Target Settings"].PrefixPosition(), targetSettings => targetSettings
                            .Action("Index", "Admin", _routeValues)
                            .Permission(OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan)
                            .LocalNav()
                        )
                    )
                )
            );

        return Task.CompletedTask;
    }
}
