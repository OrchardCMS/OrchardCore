using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Media.Deployment
{
    public class AllMediaProfilesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllMediaProfilesDeploymentStep>
    {
        public override IDisplayResult Display(AllMediaProfilesDeploymentStep step)
        {
            return
                Combine(
                    View("AllMediaProfilesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllMediaProfilesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllMediaProfilesDeploymentStep step)
        {
            return View("AllMediaProfilesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
