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
                    View("ThemesDeploymentStep_Summary", step).Location("Summary", "Content"),
                    View("ThemesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ThemesDeploymentStep step)
        {
            return View("ThemesDeploymentStep_Edit", step).Location("Content");
        }
    }
}
