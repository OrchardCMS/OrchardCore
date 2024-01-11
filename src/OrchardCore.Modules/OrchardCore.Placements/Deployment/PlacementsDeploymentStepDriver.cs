using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Placements.Deployment
{
    public class PlacementsDeploymentStepDriver : DisplayDriver<DeploymentStep, PlacementsDeploymentStep>
    {
        public override IDisplayResult Display(PlacementsDeploymentStep step)
        {
            return
                Combine(
                    View("PlacementsDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("PlacementsDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(PlacementsDeploymentStep step)
        {
            return View("PlacementsDeploymentStep_Edit", step).Location("Content");
        }
    }
}
