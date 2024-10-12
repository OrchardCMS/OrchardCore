using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps;

public sealed class CustomFileDeploymentStepDriver
    : DeploymentStepDriverBase<CustomFileDeploymentStep>
{
    public override IDisplayResult Edit(CustomFileDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<CustomFileDeploymentStepViewModel>(EditShape, model =>
        {
            model.FileContent = step.FileContent;
            model.FileName = step.FileName;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent);

        return Edit(step, context);
    }
}
