using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Workflows.Deployment
{
    public class AllWorkflowTypeDeploymentStepDriver : DisplayDriver<DeploymentStep, AllWorkflowTypeDeploymentStep>
    {
        public override IDisplayResult Display(AllWorkflowTypeDeploymentStep step)
        {
            return
                Combine(
                    View("AllWorkflowTypeDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllWorkflowTypeDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllWorkflowTypeDeploymentStep step)
        {
            return View("AllWorkflowTypeDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
