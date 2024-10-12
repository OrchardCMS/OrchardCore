using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment;

public sealed class AllAdminTemplatesDeploymentStepDriver
    : DeploymentStepDriverBase<AllAdminTemplatesDeploymentStep>
{
    public override IDisplayResult Edit(AllAdminTemplatesDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllAdminTemplatesDeploymentStepViewModel>("AllAdminTemplatesDeploymentStep_Fields_Edit", model =>
        {
            model.ExportAsFiles = step.ExportAsFiles;
        }).Location("Content");
    }
    public override async Task<IDisplayResult> UpdateAsync(AllAdminTemplatesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

        return Edit(step, context);
    }
}
