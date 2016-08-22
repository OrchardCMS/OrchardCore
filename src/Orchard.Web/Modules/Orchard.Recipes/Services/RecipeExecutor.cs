using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
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
    public class RecipeExecutor : IRecipeExecutor
    {
        private readonly IEventBus _eventBus;
        private readonly ISession _session;
        private readonly IRecipeStepExecutor _recipeStepExecutor;
        private readonly ILogger _logger;

        public RecipeExecutor(
            IEventBus eventBus,
            ISession session,
            IRecipeStepExecutor recipeStepExecutor,
            ILogger<RecipeManager> logger)
        {
            _eventBus = eventBus;
            _session = session;
            _recipeStepExecutor = recipeStepExecutor;
            _logger = logger;
        }

        public async Task ExecuteAsync(string executionId)
        {
            _eventBus.Notify<IRecipeExecuteEventHandler>(x =>
                x.ExecutionStartAsync(executionId));

            try
            {
                while (
                    await _recipeStepExecutor.ExecuteNextStepAsync(executionId)) {
                }

                _eventBus.Notify<IRecipeExecuteEventHandler>(x =>
                    x.ExecutionCompleteAsync(executionId));
            }
            catch (Exception)
            {
                _eventBus.Notify<IRecipeExecuteEventHandler>(x =>
                    x.ExecutionFailedAsync(executionId));

                throw;
            }
        }
    }
}
