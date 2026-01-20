using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public interface IRecipeExecutor
    {
        Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor, IDictionary<string, object> environment, CancellationToken cancellationToken);
    }
}
