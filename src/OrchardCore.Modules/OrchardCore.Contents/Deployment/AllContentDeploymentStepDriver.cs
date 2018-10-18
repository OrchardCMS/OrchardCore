using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment
{
    public class AllContentDeploymentStepDriver : DisplayDriver<DeploymentStep, AllContentDeploymentStep>
    {
        public override IDisplayResult Display(AllContentDeploymentStep step)
        {
            return
                Combine(
                    View("AllContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllContentDeploymentStep step)
        {
            return View("AllContentDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
