using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.Recipes.Models;

namespace Orchard.Setup.Services
{
    public interface ISetupService
    {
        Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync();
        Task<string> SetupAsync(SetupContext context);
    }
}