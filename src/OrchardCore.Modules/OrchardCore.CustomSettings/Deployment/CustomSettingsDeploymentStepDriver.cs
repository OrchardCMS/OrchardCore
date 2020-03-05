using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.CustomSettings.Services;
using OrchardCore.CustomSettings.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.CustomSettings.Deployment
{
    public class CustomSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomSettingsDeploymentStep>
    {
        private readonly CustomSettingsService _customSettingsService;

        public CustomSettingsDeploymentStepDriver(CustomSettingsService customSettingsService)
        {
            _customSettingsService = customSettingsService;
        }

        public override IDisplayResult Display(CustomSettingsDeploymentStep step)
        {
            return
                Combine(
                    View("CustomSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("CustomSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
                );
        }

        public override IDisplayResult Edit(CustomSettingsDeploymentStep step)
        {
            return Initialize<CustomSettingsDeploymentStepViewModel>("CustomSettingsDeploymentStep_Fields_Edit", model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.SettingsTypeNames = step.SettingsTypeNames;
                model.AllSettingsTypeNames = _customSettingsService.GetAllSettingsTypeNames().ToArray();
            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomSettingsDeploymentStep step, IUpdateModel updater)
        {
            step.SettingsTypeNames = Array.Empty<string>();

            await updater.TryUpdateModelAsync(step,
                                              Prefix,
                                              x => x.SettingsTypeNames,
                                              x => x.IncludeAll);

            // don't have the selected option if include all
            if (step.IncludeAll)
            {
                step.SettingsTypeNames = Array.Empty<string>();
            }

            return Edit(step);
        }
    }
}
