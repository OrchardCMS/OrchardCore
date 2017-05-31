using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Steps
{
    public class AllContentDeploymentStepDriver : DisplayDriver<DeploymentStep, AllContentDeploymentStep>
    {
        public override IDisplayResult Display(AllContentDeploymentStep step)
        {
            return
                Combine(
                    Shape("AllContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    Shape("AllContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllContentDeploymentStep step)
        {
            return Shape("AllContentDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
