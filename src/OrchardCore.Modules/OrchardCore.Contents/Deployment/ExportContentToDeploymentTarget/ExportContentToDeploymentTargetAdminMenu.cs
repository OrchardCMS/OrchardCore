using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary _routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ExportContentToDeploymentTargetSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public ExportContentToDeploymentTargetAdminMenu(IStringLocalizer<AdminMenu> stringLocalizer)
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
                    .Add(S["Settings"], settings => settings
                        .Add(S["Export target"], S["Export target"].PrefixPosition(), targetSettings => targetSettings
                            .Action("Index", "Admin", _routeValues)
                            .Permission(OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan)
                            .LocalNav()
                        )
                    )
                )
            );

            return ValueTask.CompletedTask;
        }

        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Deployment Targets"], S["Deployment Targets"].PrefixPosition(), targetSettings => targetSettings
                    .Action("Index", "Admin", _routeValues)
                    .Permission(OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
