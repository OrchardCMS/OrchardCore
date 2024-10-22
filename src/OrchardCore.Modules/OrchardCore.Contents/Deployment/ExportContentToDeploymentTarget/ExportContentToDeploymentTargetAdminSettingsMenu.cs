using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetAdminSettingsMenu : SettingsNavigationProvider
{
    internal readonly IStringLocalizer S;

    public ExportContentToDeploymentTargetAdminSettingsMenu(IStringLocalizer<AdminMenu> stringLocalizer)
    {
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(S["General"], general => general
                .Add(S["Export Target Settings"], S["Export Target Settings"].PrefixPosition(), targetSettings => targetSettings
                    .Action(GetRouteValues(ExportContentToDeploymentTargetSettingsDisplayDriver.GroupId))
                    .Permission(OrchardCore.Deployment.CommonPermissions.ManageDeploymentPlan)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
