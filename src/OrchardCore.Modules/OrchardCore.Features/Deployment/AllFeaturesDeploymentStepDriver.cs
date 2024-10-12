using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Features.ViewModels;

namespace OrchardCore.Features.Deployment;

public sealed class AllFeaturesDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AllFeaturesDeploymentStep, AllFeaturesDeploymentStepViewModel>
{
    public AllFeaturesDeploymentStepDriver(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    public override IDisplayResult Edit(AllFeaturesDeploymentStep step, Action<AllFeaturesDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, model =>
        {
            model.IgnoreDisabledFeatures = step.IgnoreDisabledFeatures;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(AllFeaturesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.IgnoreDisabledFeatures);

        return Edit(step, context);
    }
}
