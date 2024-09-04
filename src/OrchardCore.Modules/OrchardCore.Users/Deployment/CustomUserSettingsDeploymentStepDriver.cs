using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Deployment;

public sealed class CustomUserSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, CustomUserSettingsDeploymentStep>
{
    private readonly CustomUserSettingsService _customUserSettingsService;

    public CustomUserSettingsDeploymentStepDriver(CustomUserSettingsService customUserSettingsService)
    {
        _customUserSettingsService = customUserSettingsService;
    }

    public override Task<IDisplayResult> DisplayAsync(CustomUserSettingsDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
                View("CustomUserSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("CustomUserSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content"));
    }

    public override IDisplayResult Edit(CustomUserSettingsDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<CustomUserSettingsDeploymentStepViewModel>("CustomUserSettingsDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.SettingsTypeNames = step.SettingsTypeNames;
            model.AllSettingsTypeNames = (await _customUserSettingsService.GetAllSettingsTypeNamesAsync()).ToArray();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomUserSettingsDeploymentStep step, UpdateEditorContext context)
    {
        step.SettingsTypeNames = [];
        await context.Updater.TryUpdateModelAsync(step, Prefix);

        if (step.IncludeAll)
        {
            step.SettingsTypeNames = [];
        }

        return Edit(step, context);
    }
}
