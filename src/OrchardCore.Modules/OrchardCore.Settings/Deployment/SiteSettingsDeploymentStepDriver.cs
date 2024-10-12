using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Deployment;

public sealed class SiteSettingsDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<SiteSettingsDeploymentStep>
{
    public SiteSettingsDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override IDisplayResult Edit(SiteSettingsDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<SiteSettingsDeploymentStepViewModel>(EditShape, model =>
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
