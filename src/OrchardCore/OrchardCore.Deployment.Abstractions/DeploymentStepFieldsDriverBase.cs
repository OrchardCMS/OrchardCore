using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment;

public abstract class DeploymentStepFieldsDriverBase<TStep>
    : DisplayDriver<DeploymentStep, TStep> where TStep : DeploymentStep
{
    protected IServiceProvider ServiceProvider;
    protected readonly string DisplaySummaryShape;
    protected readonly string DisplayThumbnailShape;
    protected readonly string EditShape;

    protected DeploymentStepFieldsDriverBase(IServiceProvider serviceProvider)
    {
        var stepName = typeof(TStep).Name;
        ServiceProvider = serviceProvider;
        DisplaySummaryShape = $"{stepName}_Fields_Summary";
        DisplayThumbnailShape = $"{stepName}_Fields_Thumbnail";
        EditShape = $"{stepName}_Edit";
    }

    public override Task<IDisplayResult> DisplayAsync(TStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View(DisplaySummaryShape, step).Location("Summary", "Content"),
                View(DisplayThumbnailShape, step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(TStep step, BuildEditorContext context)
        => View(EditShape, step).Location("Content");
}
