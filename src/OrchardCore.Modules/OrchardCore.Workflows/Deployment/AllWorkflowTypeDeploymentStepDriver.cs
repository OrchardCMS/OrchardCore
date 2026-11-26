using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Workflows.Deployment;

public sealed class AllWorkflowTypeDeploymentStepDriver : DisplayDriver<DeploymentStep, AllWorkflowTypeDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public AllWorkflowTypeDeploymentStepDriver(IStringLocalizer<AllWorkflowTypeDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(AllWorkflowTypeDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllWorkflowTypeDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("AllWorkflowTypeDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllWorkflowTypeDeploymentStep step, BuildEditorContext context)
    {
        return View("AllWorkflowTypeDeploymentStep_Fields_Edit", step).Location(l => l.Zone("Content").Card(S["Workflows"], "10"));
    }
}
