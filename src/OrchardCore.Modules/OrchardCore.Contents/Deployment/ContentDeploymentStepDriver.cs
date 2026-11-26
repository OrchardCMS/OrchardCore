using Microsoft.Extensions.Localization;
using OrchardCore.Contents.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Deployment;

public sealed class ContentDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentDeploymentStep>
{
    internal readonly IStringLocalizer S;

    public ContentDeploymentStepDriver(IStringLocalizer<ContentDeploymentStepDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override Task<IDisplayResult> DisplayAsync(ContentDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ContentDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
                View("ContentDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ContentDeploymentStep step, BuildEditorContext context)
    {
        return Combine(
            Initialize<ContentDeploymentStepViewModel>("ContentDeploymentStep_Fields_Edit", model => PopulateViewModel(step, model))
                .Location(l => l.Zone("Content", "1").Card(S["Content types"], "10")),
            Initialize<ContentDeploymentStepViewModel>("ContentDeploymentStepExport_Fields_Edit", model => PopulateViewModel(step, model))
                .Location(l => l.Zone("Content", "2").Card(S["Export"], "20")));
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentDeploymentStep step, UpdateEditorContext context)
    {
        // Initializes the value to empty otherwise the model is not updated if no type is selected.
        step.ContentTypes = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.ContentTypes, x => x.ExportAsSetupRecipe);

        return Edit(step, context);
    }

    private static void PopulateViewModel(ContentDeploymentStep step, ContentDeploymentStepViewModel model)
    {
        model.ContentTypes = step.ContentTypes;
        model.ExportAsSetupRecipe = step.ExportAsSetupRecipe;
    }
}
