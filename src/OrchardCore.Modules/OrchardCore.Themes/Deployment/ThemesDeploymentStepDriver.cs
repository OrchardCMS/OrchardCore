using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Themes.Deployment
{
    public class ThemesDeploymentStepDriver : DisplayDriver<DeploymentStep, ThemesDeploymentStep>
    {
        public override IDisplayResult Display(ThemesDeploymentStep step)
        {
            return
                Combine(
                    Shape("ThemesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    Shape("ThemesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ThemesDeploymentStep step)
        {
            return Shape("ThemesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
