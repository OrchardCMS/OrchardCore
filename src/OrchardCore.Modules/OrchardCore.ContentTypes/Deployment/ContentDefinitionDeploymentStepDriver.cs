using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Deployment
{
    public class ContentDefinitionDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentDefinitionDeploymentStep>
    {
        public override IDisplayResult Display(ContentDefinitionDeploymentStep step)
        {
            return
                Combine(
                    Copy("ContentDefinitionDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    Copy("ContentDefinitionDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ContentDefinitionDeploymentStep step)
        {
            return Copy("ContentDefinitionDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
