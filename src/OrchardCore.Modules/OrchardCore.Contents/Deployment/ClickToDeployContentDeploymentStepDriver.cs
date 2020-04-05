using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment
{
    public class ClickToDeployContentDeploymentStepDriver : DisplayDriver<DeploymentStep, ClickToDeployContentDeploymentStep>
    {
        public override IDisplayResult Display(ClickToDeployContentDeploymentStep step)
        {
            return
                Combine(
                    View("ClickToDeployContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ClickToDeployContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ClickToDeployContentDeploymentStep step)
        {
            return View("ClickToDeployContentDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
