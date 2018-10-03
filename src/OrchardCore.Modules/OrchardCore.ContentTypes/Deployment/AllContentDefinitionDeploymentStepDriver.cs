using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Deployment
{
    public class AllContentDefinitionDeploymentStepDriver : DisplayDriver<DeploymentStep, AllContentDefinitionDeploymentStep>
    {
        public override IDisplayResult Display(AllContentDefinitionDeploymentStep step)
        {
            return
                Combine(
                    View("AllContentDefinitionDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("AllContentDefinitionDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(AllContentDefinitionDeploymentStep step)
        {
            return View("AllContentDefinitionDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
