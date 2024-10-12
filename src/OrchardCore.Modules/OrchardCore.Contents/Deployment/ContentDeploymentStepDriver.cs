using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment;

public sealed class ContentDeploymentStepDriver
    : DeploymentStepFieldsDriverBase<ContentDeploymentStep>
{
    public ContentDeploymentStepDriver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
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
