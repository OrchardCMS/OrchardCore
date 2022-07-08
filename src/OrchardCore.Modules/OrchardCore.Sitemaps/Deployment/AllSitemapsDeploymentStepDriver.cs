using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Sitemaps.Deployment
{
    public class AllSitemapsDeploymentStepDriver : DisplayDriver<DeploymentStep, AllSitemapsDeploymentStep>
    {
        public override IDisplayResult Display(AllSitemapsDeploymentStep step)
        {
            return
                Combine(
                    View("AllSitemapsDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllSitemapsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllSitemapsDeploymentStep step)
        {
            return View("AllSitemapsDeploymentStep_Edit", step).Location("Content");
        }
    }
}
