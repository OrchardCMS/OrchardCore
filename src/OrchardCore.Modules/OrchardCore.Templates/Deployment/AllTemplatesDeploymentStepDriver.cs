using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment;

public sealed class AllTemplatesDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<AllTemplatesDeploymentStep, AllTemplatesDeploymentStepViewModel>
{
    public AllTemplatesDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public override IDisplayResult Edit(AllTemplatesDeploymentStep step, Action<AllTemplatesDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, model =>
        {
            model.ExportAsFiles = step.ExportAsFiles;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(AllTemplatesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

        return Edit(step, context);
    }
}
