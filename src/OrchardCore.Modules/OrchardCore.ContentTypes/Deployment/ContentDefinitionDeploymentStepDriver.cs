using OrchardCore.ContentTypes.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Deployment;

public sealed class ContentDefinitionDeploymentStepDriver : DisplayDriver<DeploymentStep, ContentDefinitionDeploymentStep>
{
    public override Task<IDisplayResult> DisplayAsync(ContentDefinitionDeploymentStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View("ContentDefinitionDeploymentStep_Fields_Summary", step).Location("Summary", "Content"),
                View("ContentDefinitionDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(ContentDefinitionDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<ContentDefinitionStepViewModel>("ContentDefinitionDeploymentStep_Fields_Edit", model =>
        {
            model.ContentParts = step.ContentParts;
            model.ContentTypes = step.ContentTypes;
            model.IncludeAll = step.IncludeAll;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentDefinitionDeploymentStep step, UpdateEditorContext context)
    {
        // Initializes the value to empty otherwise the model is not updated if no type is selected.
        step.ContentTypes = [];
        step.ContentParts = [];

        await context.Updater.TryUpdateModelAsync(
            step,
            Prefix,
            x => x.ContentTypes,
            x => x.ContentParts,
            x => x.IncludeAll);

        // don't have the selected option if include all
        if (step.IncludeAll)
        {
            step.ContentTypes = [];
            step.ContentParts = [];
        }
        else
        {
            step.ContentParts = step.ContentParts.Distinct().ToArray();
        }

        return Edit(step, context);
    }
}
