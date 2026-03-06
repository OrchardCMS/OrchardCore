using Microsoft.Extensions.Options;
using OrchardCore.Deployment.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Deployment.Steps;

/// <summary>
/// Display driver for <see cref="RecipeStepDeploymentStep"/> that renders generic
/// Summary, Thumbnail, and Edit views using metadata from <see cref="RecipeStepDeploymentOptions"/>.
/// </summary>
public sealed class RecipeStepDeploymentStepDriver : DisplayDriver<DeploymentStep, RecipeStepDeploymentStep>
{
    private readonly RecipeStepDeploymentOptions _options;

    public RecipeStepDeploymentStepDriver(IOptions<RecipeStepDeploymentOptions> options)
    {
        _options = options.Value;
    }

    public override Task<IDisplayResult> DisplayAsync(RecipeStepDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            Initialize<RecipeStepDeploymentStepViewModel>("RecipeStepDeploymentStep_Fields_Summary", m => BuildViewModel(step, m))
                .Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            Initialize<RecipeStepDeploymentStepViewModel>("RecipeStepDeploymentStep_Fields_Thumbnail", m => BuildViewModel(step, m))
                .Location("Thumbnail", "Content")
        );
    }

    public override IDisplayResult Edit(RecipeStepDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<RecipeStepDeploymentStepViewModel>("RecipeStepDeploymentStep_Fields_Edit", m => BuildViewModel(step, m))
            .Location("Content");
    }

    private void BuildViewModel(RecipeStepDeploymentStep step, RecipeStepDeploymentStepViewModel model)
    {
        if (_options.Steps.TryGetValue(step.RecipeStepName, out var info))
        {
            model.Title = info.Title;
            model.Description = info.Description;
        }
        else
        {
            model.Title = step.RecipeStepName;
            model.Description = $"Exports {step.RecipeStepName} data.";
        }
    }
}
