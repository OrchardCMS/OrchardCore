using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment;

public abstract class DeploymentStepDriverBase<TStep>
    : DisplayDriver<DeploymentStep, TStep> where TStep : DeploymentStep
{
    private readonly string _stepName = typeof(TStep).Name;

    public override Task<IDisplayResult> DisplayAsync(TStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View($"{_stepName}_Summary", step).Location("Summary", "Content"),
                View($"{_stepName}_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(TStep step, BuildEditorContext context)
        => View($"{_stepName}_Edit", step).Location("Content");
}
