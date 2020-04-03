using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Templates.Deployment
{
    public class AllTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllTemplatesDeploymentStep>
    {
        public override IDisplayResult Display(AllTemplatesDeploymentStep step)
        {
            return
                Combine(
                    View("AllTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllTemplatesDeploymentStep step)
        {
            return View("AllTemplatesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
