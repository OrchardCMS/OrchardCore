using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment
{
    public class GenericSiteSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, GenericSiteSettingsDeploymentStep>
    {
        public override IDisplayResult Display(GenericSiteSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("GenericSiteSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("GenericSiteSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(GenericSiteSettingsDeploymentStep step)
        {
            return Initialize<GenericSiteSettingsDeploymentStepViewModel>("GenericSiteSettingsDeploymentStep_Fields_Edit", model =>
            {
                model.Title = step.Title;
                model.Description = step.Description;
            }).Location("Content");
        }
    }
}
