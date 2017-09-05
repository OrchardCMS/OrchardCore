using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public interface IRecipeStore
    {
        Task CreateAsync(RecipeResult recipeResult);
        
        Task DeleteAsync(RecipeResult recipeResult);
        
        Task<RecipeResult> FindByExecutionIdAsync(string executionId);

        Task UpdateAsync(RecipeResult recipeResult);
    }
}
