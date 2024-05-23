using System;

namespace OrchardCore.Recipes.Models;

public class RecipeExecutionException : Exception
{
    public RecipeStepResult StepResult { get; }

    public RecipeExecutionException(RecipeStepResult recipeStepResult)
    {
        StepResult = recipeStepResult;
    }

    public RecipeExecutionException(Exception exception, RecipeStepResult recipeStepResult)
        : base(exception.Message, exception)
    {
        StepResult = recipeStepResult;
    }
}
