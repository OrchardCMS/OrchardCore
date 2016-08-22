using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.State;
using Orchard.Events;
using Orchard.FileSystem;
using Orchard.Recipes.Events;
using Orchard.Recipes.Models;
using System;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace Orchard.Recipes.Services
{
    public class RecipeManager : IRecipeManager
    {
        private readonly IRecipeExecutor _recipeExecutor;
        private readonly IRecipeQueue _recipeQueue;

        private readonly ILogger _logger;

        public RecipeManager(
            IRecipeExecutor recipeExecutor,
            IRecipeQueue recipeQueue,
            ILogger<RecipeManager> logger)
        {
            _recipeExecutor = recipeExecutor;
            _recipeQueue = recipeQueue;

            _logger = logger;
        }

        public async Task ExecuteAsync(string executionId)
        {
            // todo (ngm) check if recipe enqueued

            await _recipeExecutor.ExecuteAsync(executionId);
        }

        public async Task<string> EnqueueAsync(RecipeDescriptor recipeDescriptor)
        {
            var executionId = Guid.NewGuid().ToString("n");

            return await EnqueueAsync(executionId, recipeDescriptor);
        }

        public async Task<string> EnqueueAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            return await _recipeQueue.EnqueueAsync(executionId, recipeDescriptor);
        }
    }
}
