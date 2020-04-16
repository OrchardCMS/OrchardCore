using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ThemeSettings.Deployment
{
    public class ThemeSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, ThemeSettingsDeploymentStep>
    {
        public override IDisplayResult Display(ThemeSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("ThemeSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("ThemeSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(ThemeSettingsDeploymentStep step)
        {
            return View("ThemeSettingsDeploymentStep_Fields_Edit", step).Location("Content");
        }
    }
}
