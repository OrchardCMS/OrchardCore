using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.Contents.Deployment.ExportContentToDeploymentTarget;

public sealed class ExportContentToDeploymentTargetContentDriver : ContentDisplayDriver
{
    private readonly IDeploymentPlanService _deploymentPlanService;
    private readonly ISiteService _siteService;

    public ExportContentToDeploymentTargetContentDriver(
        IDeploymentPlanService deploymentPlanService,
        ISiteService siteService)
    {
        _deploymentPlanService = deploymentPlanService;
        _siteService = siteService;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
    {
        return CombineAsync(
            Dynamic("ExportContentToDeploymentTarget_Modal__ActionDeploymentTarget")
                .Location("SummaryAdmin", "ActionsMenu:30")
                .RenderWhen(async () =>
                {
                    if (await _deploymentPlanService.DoesUserHaveExportPermissionAsync())
                    {
                        var exportContentToDeploymentTargetSettings = await _siteService.GetSettingsAsync<ExportContentToDeploymentTargetSettings>();

                        if (exportContentToDeploymentTargetSettings.ExportContentToDeploymentTargetPlanId != 0)
                        {
                            return true;
                        }
                    }

                    return false;
                }),
            Shape("ExportContentToDeploymentTarget_SummaryAdmin__Button__Actions", new ContentItemViewModel(model))
                .Location("SummaryAdmin", "ActionsMenu:40")
                .RenderWhen(async () =>
                {
                    if (await _deploymentPlanService.DoesUserHaveExportPermissionAsync())
                    {
                        var exportContentToDeploymentTargetSettings = await _siteService.GetSettingsAsync<ExportContentToDeploymentTargetSettings>();

                        if (exportContentToDeploymentTargetSettings.ExportContentToDeploymentTargetPlanId != 0)
                        {
                            return true;
                        }
                    }

                    return false;
                })
            );
    }
}
