using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public interface IRecipeManager
    {
        Task<string> ExecuteAsync(RecipeDescriptor recipe);
    }
}