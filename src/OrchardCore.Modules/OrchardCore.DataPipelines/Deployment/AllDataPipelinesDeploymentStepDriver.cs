using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Deployment;

public sealed class AllDataPipelinesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllDataPipelinesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllDataPipelinesDeploymentStep step, BuildDisplayContext context)
        => CombineAsync(
            View("AllDataPipelinesDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("AllDataPipelinesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content"));

    public override IDisplayResult Edit(AllDataPipelinesDeploymentStep step, BuildEditorContext context)
        => View("AllDataPipelinesDeploymentStep_Fields_Edit", step).Location("Content");
}
