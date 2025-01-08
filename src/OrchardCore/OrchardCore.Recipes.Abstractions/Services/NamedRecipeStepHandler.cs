using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

public abstract class NamedRecipeStepHandler : IRecipeStepHandler
{
    /// <summary>
    /// The name of the recipe step.
    /// </summary>
    protected readonly string StepName;

    protected NamedRecipeStepHandler(string stepName)
    {
        StepName = stepName;
    }

    public Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, StepName, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        return HandleAsync(context);
    }

    protected abstract Task HandleAsync(RecipeExecutionContext context);
}
