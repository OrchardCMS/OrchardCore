using OrchardCore.CustomSettings.Services;
using OrchardCore.CustomSettings.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.CustomSettings.Deployment;

public sealed class CustomSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomSettingsDeploymentStep>
{
    private readonly CustomSettingsService _customSettingsService;

    public CustomSettingsDeploymentStepDriver(CustomSettingsService customSettingsService)
    {
        _customSettingsService = customSettingsService;
    }

    public override Task<IDisplayResult> DisplayAsync(CustomSettingsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("CustomSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("CustomSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(CustomSettingsDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<CustomSettingsDeploymentStepViewModel>("CustomSettingsDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.SettingsTypeNames = step.SettingsTypeNames;
            model.AllSettingsTypeNames = (await _customSettingsService.GetAllSettingsTypeNamesAsync()).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomSettingsDeploymentStep step, UpdateEditorContext context)
    {
        step.SettingsTypeNames = [];

        await context.Updater.TryUpdateModelAsync(step,
                                          Prefix,
                                          x => x.SettingsTypeNames,
                                          x => x.IncludeAll);

        // Don't have the selected option if include all.
        if (step.IncludeAll)
        {
            step.SettingsTypeNames = [];
        }

        return Edit(step, context);
    }
}
