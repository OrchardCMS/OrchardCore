namespace Orchard.Recipes.Services
{
    public interface IRecipeStepExecutor
    {
        bool ExecuteNextStep(string executionId);
    }
}