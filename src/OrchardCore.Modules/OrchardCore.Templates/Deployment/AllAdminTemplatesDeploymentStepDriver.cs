using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment;

public sealed class AllAdminTemplatesDeploymentStepDriver
    : DeploymentStepDriverBase<AllAdminTemplatesDeploymentStep, AllAdminTemplatesDeploymentStepViewModel>
{
    public override IDisplayResult Edit(AllAdminTemplatesDeploymentStep step, Action<AllAdminTemplatesDeploymentStepViewModel> intializeAction)
    {
        return base.Edit(step, model =>
        {
            model.ExportAsFiles = step.ExportAsFiles;
        });
    }

    public override async Task<IDisplayResult> UpdateAsync(AllAdminTemplatesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

        return Edit(step, context);
    }
}
