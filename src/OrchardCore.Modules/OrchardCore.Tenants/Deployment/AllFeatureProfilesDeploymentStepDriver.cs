using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Tenants.Deployment
{
    public class AllFeatureProfilesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllFeatureProfilesDeploymentStep>
    {
        public override IDisplayResult Display(AllFeatureProfilesDeploymentStep step)
        {
            return
                Combine(
                    View("AllFeatureProfilesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllFeatureProfilesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllFeatureProfilesDeploymentStep step)
        {
            return View("AllFeatureProfilesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
