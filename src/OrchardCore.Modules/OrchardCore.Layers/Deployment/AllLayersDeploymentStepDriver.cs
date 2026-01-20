using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Layers.Deployment
{
    public class AllLayersDeploymentStepDriver : DisplayDriver<DeploymentStep, AllLayersDeploymentStep>
    {
        public override IDisplayResult Display(AllLayersDeploymentStep step)
        {
            return
                Combine(
                    View("AllLayersDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllLayersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllLayersDeploymentStep step)
        {
            return View("AllLayersDeploymentStep_Edit", step).Location("Content");
        }
    }
}
