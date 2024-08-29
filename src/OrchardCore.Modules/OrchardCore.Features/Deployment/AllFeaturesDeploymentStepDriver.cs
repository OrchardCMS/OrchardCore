using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Features.ViewModels;

namespace OrchardCore.Features.Deployment;

public sealed class AllFeaturesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllFeaturesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllFeaturesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllFeaturesDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("AllFeaturesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllFeaturesDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllFeaturesDeploymentStepViewModel>("AllFeaturesDeploymentStep_Fields_Edit", model =>
        {
            model.IgnoreDisabledFeatures = step.IgnoreDisabledFeatures;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(AllFeaturesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.IgnoreDisabledFeatures);

        return Edit(step, context);
    }
}
