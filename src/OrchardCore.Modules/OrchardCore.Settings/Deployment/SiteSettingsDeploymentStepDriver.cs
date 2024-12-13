using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment;

public sealed class SiteSettingsDeploymentStepDriver : DisplayDriver<DeploymentStep, SiteSettingsDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(SiteSettingsDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("SiteSettingsDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("SiteSettingsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(SiteSettingsDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<SiteSettingsDeploymentStepViewModel>("SiteSettingsDeploymentStep_Fields_Edit", model =>
        {
            model.Settings = step.Settings;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(SiteSettingsDeploymentStep step, UpdateEditorContext context)
    {
        // Initializes the value to empty otherwise the model is not updated if no type is selected.
        step.Settings = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.Settings);

        return Edit(step, context);
    }
}
