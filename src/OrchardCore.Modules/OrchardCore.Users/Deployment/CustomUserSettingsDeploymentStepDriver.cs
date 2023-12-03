using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Deployment
{
    public class CustomUserSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomUserSettingsDeploymentStep>
    {
        private readonly CustomUserSettingsService _customUserSettingsService;

        public CustomUserSettingsDeploymentStepDriver(CustomUserSettingsService customUserSettingsService)
        {
            _customUserSettingsService = customUserSettingsService;
        }

        public override IDisplayResult Display(CustomUserSettingsDeploymentStep step)
        {
            return Combine(
                    View("CustomUserSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                    View("CustomUserSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content"));
        }

        public override IDisplayResult Edit(CustomUserSettingsDeploymentStep step)
        {
            return Initialize<CustomUserSettingsDeploymentStepViewModel>("CustomUserSettingsDeploymentStep_Fields_Edit", async model =>
            {
                model.IncludeAll = step.IncludeAll;
                model.SettingsTypeNames = step.SettingsTypeNames;
                model.AllSettingsTypeNames = (await _customUserSettingsService.GetAllSettingsTypeNamesAsync()).ToArray();
            })
            .Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(CustomUserSettingsDeploymentStep step, IUpdateModel updater)
        {
            step.SettingsTypeNames = Array.Empty<string>();
            await updater.TryUpdateModelAsync(step, Prefix);

            if (step.IncludeAll)
            {
                step.SettingsTypeNames = Array.Empty<string>();
            }

            return Edit(step);
        }
    }
}
