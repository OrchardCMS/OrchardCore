using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment;

public abstract class DeploymentStepFieldsDriverBase<TStep>
    : DisplayDriver<DeploymentStep, TStep> where TStep : DeploymentStep
{
    private readonly string _stepName = typeof(TStep).Name;

    protected IServiceProvider ServiceProvider;

    protected DeploymentStepFieldsDriverBase(IServiceProvider serviceProvider)
        => ServiceProvider = serviceProvider;

    public override Task<IDisplayResult> DisplayAsync(TStep step, BuildDisplayContext context)
    {
        return
            CombineAsync(
                View($"{_stepName}_Fields_Summary", step).Location("Summary", "Content"),
                View($"{_stepName}_Fields_Thumbnail", step).Location("Thumbnail", "Content")
            );
    }

    public override IDisplayResult Edit(TStep step, BuildEditorContext context)
        => View($"{_stepName}_Edit", step).Location("Content");
}
