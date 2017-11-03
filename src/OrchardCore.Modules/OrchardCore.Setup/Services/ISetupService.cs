using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Setup.Services
{
    public interface ISetupService
    {
        Task<IEnumerable<RecipeDescriptor>> GetSetupRecipesAsync();
        Task<string> SetupAsync(SetupContext context);
    }
}