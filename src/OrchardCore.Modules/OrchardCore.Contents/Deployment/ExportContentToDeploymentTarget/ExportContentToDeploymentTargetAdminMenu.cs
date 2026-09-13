using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetAdminMenu : AdminNavigationProvider
{
    private static readonly RouteValueDictionary s_routeValues = new()
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
        builder
            .Add(S["Settings"], settings => settings
                .Add(S["Deployment Targets"], S["Deployment Targets"].PrefixPosition(), targetSettings => targetSettings
                    .Action("Index", "Admin", s_routeValues)
                    .Permission(DeploymentPermissions.ManageDeploymentPlan)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
