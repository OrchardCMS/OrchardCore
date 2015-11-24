using Orchard.DependencyInjection;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeStepExecutor : IDependency
    {
        bool ExecuteNextStep(string executionId);
    }
}