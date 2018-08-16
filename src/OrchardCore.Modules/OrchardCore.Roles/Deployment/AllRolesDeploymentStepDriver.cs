using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Roles.Deployment
{
    public class AllRolesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllRolesDeploymentStep>
    {
        public override IDisplayResult Display(AllRolesDeploymentStep step)
        {
            return
                Combine(
                    View("AllRolesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllRolesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllRolesDeploymentStep step)
        {
            return View("AllRolesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
