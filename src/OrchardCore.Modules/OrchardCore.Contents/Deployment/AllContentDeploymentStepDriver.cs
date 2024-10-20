using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment;

public sealed class AllContentDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AllContentDeploymentStep, AllContentDeploymentStepViewModel>
{
    public AllContentDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override IDisplayResult Edit(AllContentDeploymentStep step, Action<AllContentDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, model =>
        {
            model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(AllContentDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsSetupRecipe);

        return Edit(step, context);
    }
}
