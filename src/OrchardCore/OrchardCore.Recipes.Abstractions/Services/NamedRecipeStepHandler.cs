using Annotype;
using OrchardCore.Modules;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// A recipe step handler that processes the steps named by its <see cref="RecipeStepAttribute"/>.
/// </summary>
/// <remarks>
/// Every concrete implementation must declare a <see cref="RecipeStepAttribute"/>, which is enforced at compile time.
/// </remarks>
[RequiresMetadata(typeof(RecipeStepAttribute))]
public abstract class NamedRecipeStepHandler : IRecipeStepHandler
{
    /// <summary>
    /// The name of the recipe step.
    /// </summary>
    protected readonly string StepName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedRecipeStepHandler"/> class with the step name declared by
    /// the <see cref="RecipeStepAttribute"/> of the handler.
    /// </summary>
    protected NamedRecipeStepHandler()
    {
        StepName = TypeMetadata.Get<RecipeStepAttribute>(GetType()).Name;
    }

    [Obsolete("Declare the step name with a [RecipeStep] attribute and use the parameterless constructor. This constructor will be removed in a future version.")]
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
