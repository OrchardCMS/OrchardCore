using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeStepExecutor
    {
        Task<bool> ExecuteNextStepAsync(string executionId);
    }
}