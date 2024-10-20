using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps;

public sealed class CustomFileDeploymentStepFieldsDriver
    : DeploymentStepFieldsDriverBase<CustomFileDeploymentStep, CustomFileDeploymentStepViewModel>
{
    public CustomFileDeploymentStepFieldsDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override IDisplayResult Edit(CustomFileDeploymentStep step, Action<CustomFileDeploymentStepViewModel> initializeAction)
    {
        return base.Edit(step, model =>
        {
            model.FileName = step.FileName;
            model.FileContent = step.FileContent;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(CustomFileDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.FileName, x => x.FileContent);

        return Edit(step, context);
    }
}
