using Orchard.DependencyInjection;
using Orchard.Environment.Recipes.Models;

namespace Orchard.Environment.Recipes.Services
{
    public interface IRecipeManager : IDependency
    {
        string Execute(Recipe recipe);
    }
}