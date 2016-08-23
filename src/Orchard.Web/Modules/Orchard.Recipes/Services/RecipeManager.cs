using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using System;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeManager : IRecipeManager
    {
        private readonly IRecipeExecutor _recipeExecutor;

        private readonly ILogger _logger;

        public RecipeManager(
            IRecipeExecutor recipeExecutor,
            ILogger<RecipeManager> logger)
        {
            _recipeExecutor = recipeExecutor;

            _logger = logger;
        }

        public async Task<string> ExecuteAsync(RecipeDescriptor recipeDescriptor)
        {
            var executionId = Guid.NewGuid().ToString("n");

            await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor);

            return executionId;
        }

        public async Task<string> ExecuteAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor);

            return executionId;
        }
    }
}
