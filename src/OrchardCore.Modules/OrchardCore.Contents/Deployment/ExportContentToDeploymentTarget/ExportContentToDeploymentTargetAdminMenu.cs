using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetAdminMenu : AdminNavigationProvider
{
    private readonly IStringLocalizerFactory _localizerFactory;

    private static readonly RouteValueDictionary s_routeValues = new()
    {
        { "area", "OrchardCore.Settings" },
        { "groupId", ExportContentToDeploymentTargetSettingsDisplayDriver.GroupId },
    };

    internal readonly IStringLocalizer S;

    public ExportContentToDeploymentTargetAdminMenu(IStringLocalizer<AdminMenu> stringLocalizer, IStringLocalizerFactory localizerFactory)
    {
        _localizerFactory = localizerFactory;
        S = stringLocalizer;
    }

    protected override ValueTask BuildAsync(NavigationBuilder builder)
    {
        builder
            .Add(_localizerFactory.Localize(CommonLocalizationSources.Settings), settings => settings
                .Add(S["Deployment Targets"], S["Deployment Targets"].PrefixPosition(), targetSettings => targetSettings
                    .Action("Index", "Admin", s_routeValues)
                    .Permission(DeploymentPermissions.ManageDeploymentPlan)
                    .LocalNav()
                )
            );

        return ValueTask.CompletedTask;
    }
}
