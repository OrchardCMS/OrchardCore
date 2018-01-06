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
                    Shape("AllLayersDeploymentStep_Summary", step).Location("Summary", "Content"),
                    Shape("AllLayersDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllLayersDeploymentStep step)
        {
            return Shape("AllLayersDeploymentStep_Edit", step).Location("Content");
        }
    }
}
