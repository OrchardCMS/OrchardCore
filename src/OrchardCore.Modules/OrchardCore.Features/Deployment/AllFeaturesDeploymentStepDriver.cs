using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Features.Deployment
{
    public class AllFeaturesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllFeaturesDeploymentStep>
    {
        public override IDisplayResult Display(AllFeaturesDeploymentStep step)
        {
            return
                Combine(
                    Shape("AllFeaturesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    Shape("AllFeaturesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllFeaturesDeploymentStep step)
        {
            return Shape("AllFeaturesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
