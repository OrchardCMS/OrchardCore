using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Templates.Deployment
{
    public class AllAdminTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllAdminTemplatesDeploymentStep>
    {
        public override IDisplayResult Display(AllAdminTemplatesDeploymentStep step)
        {
            return
                Combine(
                    View("AllAdminTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("AllAdminTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllAdminTemplatesDeploymentStep step)
        {
            return View("AllAdminTemplatesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
