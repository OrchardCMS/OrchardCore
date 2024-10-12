using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment;

public sealed class AllContentDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AllContentDeploymentStep>
{
    public AllContentDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override IDisplayResult Edit(AllContentDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllContentDeploymentStepViewModel>("AllContentDeploymentStep_Fields_Edit", model =>
        {
            model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(AllContentDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsSetupRecipe);

        return Edit(step, context);
    }
}
