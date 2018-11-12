using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminTrees.Deployment
{
    public class AdminTreesDeploymentStepDriver : DisplayDriver<DeploymentStep, AdminTreesDeploymentStep>
    {
        public override IDisplayResult Display(AdminTreesDeploymentStep step)
        {
            return
                Combine(
                    View("AdminTreesDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AdminTreesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AdminTreesDeploymentStep step)
        {
            return View("AdminTreesDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
