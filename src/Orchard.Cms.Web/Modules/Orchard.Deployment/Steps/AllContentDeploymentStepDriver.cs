using Orchard.Deployment.Editors;
using Orchard.DisplayManagement.Views;

namespace Orchard.Deployment.Steps
{
    public class AllContentDeploymentStepDriver : DeploymentStepDisplayDriver<AllContentDeploymentStep>
    {
        public override IDisplayResult Display(AllContentDeploymentStep step)
        {
            return
                Combine(
                    Shape("AllContentDeploymentStep", step).Location("Summary", "Content"),
                    Shape("AllContentDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllContentDeploymentStep step)
        {
            return Shape("AllContentDeploymentStep_Edit", step).Location("Content");
        }
    }
}
