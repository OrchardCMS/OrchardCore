using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeExecutor : IDependency
    {
        string Execute(Recipe recipe);
    }
}