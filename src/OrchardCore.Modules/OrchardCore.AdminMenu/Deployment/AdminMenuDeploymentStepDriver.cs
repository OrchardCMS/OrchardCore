using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.AdminMenu.Deployment
{
    public class AdminMenuDeploymentStepDriver : DisplayDriver<DeploymentStep, AdminMenuDeploymentStep>
    {
        public override IDisplayResult Display(AdminMenuDeploymentStep step)
        {
            return
                Combine(
                    View("AdminMenuDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AdminMenuDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AdminMenuDeploymentStep step)
        {
            return View("AdminMenuDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
