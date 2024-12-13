using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment;

public sealed class ContentDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(ContentDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ContentDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("ContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ContentDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ContentDeploymentStepViewModel>("ContentDeploymentStep_Fields_Edit", model =>
        {
            model.ContentTypes = step.ContentTypes;
            model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentDeploymentStep step, UpdateEditorContext context)
    {
        // Initializes the value to empty otherwise the model is not updated if no type is selected.
        step.ContentTypes = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ContentTypes, x => x.ExportAsSetupRecipe);

        return Edit(step, context);
    }
}
