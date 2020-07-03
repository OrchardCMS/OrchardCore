using System;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment
{
    public class GeneralSiteSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, GeneralSiteSettingsDeploymentStep>
    {
        public override IDisplayResult Display(GeneralSiteSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("GeneralSiteSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("GeneralSiteSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(GeneralSiteSettingsDeploymentStep step)
        {
            return Initialize<GeneralSiteSettingsDeploymentStepViewModel>("GeneralSiteSettingsDeploymentStep_Fields_Edit", model =>
            {
                model.Settings = step.Settings;
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(GeneralSiteSettingsDeploymentStep step, IUpdateModel updater)
        {
            // Initializes the value to empty otherwise the model is not updated if no type is selected.
            step.Settings = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step, Prefix, x => x.Settings);

            return Edit(step);
        }
    }
}
