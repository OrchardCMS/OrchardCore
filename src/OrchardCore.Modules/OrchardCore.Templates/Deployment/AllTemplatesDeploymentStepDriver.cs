using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.ViewModels;

namespace OrchardCore.Templates.Deployment;

public sealed class AllTemplatesDeploymentStepDriver : DisplayDriver<DeploymentStep, AllTemplatesDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(AllTemplatesDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("AllTemplatesDeploymentStep_Summary", step).Location("Summary", "Content"),
                View("AllTemplatesDeploymentStep_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(AllTemplatesDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<AllTemplatesDeploymentStepViewModel>("AllTemplatesDeploymentStep_Fields_Edit", model =>
        {
            model.ExportAsFiles = step.ExportAsFiles;
        }).Location("Content");
    }
    public override async Task<IDisplayResult> UpdateAsync(AllTemplatesDeploymentStep step, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ExportAsFiles);

        return Edit(step, context);
    }
}
