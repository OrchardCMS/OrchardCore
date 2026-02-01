using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Bridges <see cref="IRecipeDeploymentStep"/> implementations to the <see cref="IRecipeStepHandler"/> interface
/// for backwards compatibility with the existing recipe execution pipeline.
/// </summary>
/// <remarks>
/// <para>
/// This handler allows unified <see cref="IRecipeDeploymentStep"/> implementations to participate
/// in the existing recipe execution pipeline without requiring changes to <see cref="RecipeExecutor"/>.
/// </para>
/// <para>
/// <b>How it works:</b> When a recipe is executed, this handler iterates through all registered
/// <see cref="IRecipeDeploymentStep"/> implementations and calls <see cref="IRecipeDeploymentStep.ExecuteAsync"/>
/// on the one that matches the step name.
/// </para>
/// </remarks>
#pragma warning disable CS0618 // Type or member is obsolete
public sealed class RecipeDeploymentStepHandler : IRecipeStepHandler
#pragma warning restore CS0618 // Type or member is obsolete
{
    private readonly IEnumerable<IRecipeDeploymentStep> _steps;

    public RecipeDeploymentStepHandler(IEnumerable<IRecipeDeploymentStep> steps)
    {
        _steps = steps;
    }

    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var step in _steps)
        {
            if (string.Equals(step.Name, context.Name, StringComparison.OrdinalIgnoreCase))
            {
                await step.ExecuteAsync(context);
                return;
            }
        }
    }
}
